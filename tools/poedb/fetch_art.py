"""poedb.tw 美术资源摄取（装备图标 / 技能宝石图 / 辅助宝石图 / 技能特效图）。

只做两件事：按清单 GET cdn.poedb.tw 的 .webp → PIL 解码 → 写成 Unity 可导入的 .png（含 .meta）。
不抓 HTML、不改工程代码；清单即真值（人工逐条核对来源页，见 docs/reviews/S6P/S6P_WO_05_POEDB_SOURCING.md）。

用法：
    python tools/poedb/fetch_art.py            # 缺什么补什么（幂等）
    python tools/poedb/fetch_art.py --force    # 全部重下
"""

import argparse
import io
import os
import sys
import urllib.request
import uuid

from PIL import Image

UA = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)',
    'Referer': 'https://poedb.tw/us/',
}
CDN = 'https://cdn.poedb.tw/image/'

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
RES = os.path.join(REPO, 'unity', 'game', 'New Unity Project', 'Assets', 'Resources', 'UI', 'PoE')

# (目标子目录, 目标文件名, CDN 相对路径)
MANIFEST = [
    # ---- 装备图标：六槽 × 普通/稀有 ----
    ('Items', 'Weapon_Ordinary', 'Art/2DItems/Weapons/OneHandWeapons/OneHandSwords/OneHandSword1.webp'),
    ('Items', 'Weapon_Rare', 'Art/2DItems/Weapons/OneHandWeapons/OneHandSwords/OneHandSword5.webp'),
    ('Items', 'Body_Ordinary', 'Art/2DItems/Armours/BodyArmours/BodyDex1A.webp'),
    ('Items', 'Body_Rare', 'Art/2DItems/Armours/BodyArmours/BodyStr2A.webp'),
    ('Items', 'Helmet_Ordinary', 'Art/2DItems/Armours/Helmets/HelmetStrInt1.webp'),
    ('Items', 'Helmet_Rare', 'Art/2DItems/Armours/Helmets/HelmetStr4.webp'),
    ('Items', 'Gloves_Ordinary', 'Art/2DItems/Armours/Gloves/GlovesInt1.webp'),
    ('Items', 'Gloves_Rare', 'Art/2DItems/Armours/Gloves/GlovesStr5.webp'),
    ('Items', 'Boots_Ordinary', 'Art/2DItems/Armours/Boots/BootsStr1.webp'),
    ('Items', 'Boots_Rare', 'Art/2DItems/Armours/Boots/BootsInt1.webp'),
    ('Items', 'Belt_Ordinary', 'Art/2DItems/Belts/Belt1.webp'),
    ('Items', 'Belt_Rare', 'Art/2DItems/Belts/Belt7.webp'),
    # ---- 主动技能宝石图 ----
    ('Skills', 'Melee', 'Art/2DItems/Gems/HeavyStrike.webp'),
    ('Skills', 'Projectile', 'Art/2DItems/Gems/SplitArrow.webp'),
    ('Skills', 'Area', 'Art/2DItems/Gems/Firestorm.webp'),
    ('Skills', 'IceSpear', 'Art/2DItems/Gems/IceSpear.webp'),
    ('Skills', 'Fireball', 'Art/2DItems/Gems/Fireball.webp'),
    # ---- 辅助宝石图 ----
    ('Supports', 'AddedFire', 'Art/2DItems/Gems/Support/AddedFireDamage.webp'),
    ('Supports', 'Brutal', 'Art/2DItems/Gems/Support/Brutality.webp'),
    ('Supports', 'Concentrated', 'Art/2DItems/Gems/Support/ConcentratedAOE.webp'),
    ('Supports', 'Faster', 'Art/2DItems/Gems/Support/FasterAttacks.webp'),
    ('Supports', 'Combustion', 'Art/2DItems/Gems/Support/ChancetoIgnite.webp'),
    ('Supports', 'Fork', 'Art/2DItems/Gems/Support/Fork.webp'),
    ('Supports', 'FireConversion', 'Art/2DItems/Gems/Support/ColdtoFire.webp'),
    ('Supports', 'ReturningProjectiles', 'Art/2DItems/Gems/Support/ReturnProjectiles.webp'),
    ('Supports', 'SnipersMark', 'Art/2DItems/Gems/ProjectileWeakness.webp'),
    # ---- 技能特效图（poedb 发布的美术；作投射物广告牌贴图用） ----
    # `Art/2DArt/SkillIcons/*` 是不透明方图（实测 alpha 恒 255），做广告牌会带一块深色底板，
    # 故这两条走 KEYOUT 抠除近黑背景（见 keyout_background）；`Art/2DItems/Effects/*` 自带 alpha 不需处理。
    ('Vfx', 'IceSpear', 'Art/2DArt/SkillIcons/IceSpear.webp', True),
    ('Vfx', 'Fireball', 'Art/2DArt/SkillIcons/iconfireball.webp', True),
    ('Vfx', 'IceSpearMtx', 'Art/2DItems/Effects/VoidEmperorIceSpearEffect.webp'),
    ('Vfx', 'IceSpearMtxAlt', 'Art/2DItems/Effects/AuspiciousIceSpearEffect.webp'),
]
META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 0
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def load_meta_guid(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            for line in f:
                if line.startswith('guid: '):
                    return line[6:].strip()
    except OSError:
        pass
    return None


def fetch(url):
    return urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=60).read()


def keyout_background(im):
    """把近黑背景抠成透明（仅对不透明方图用）。

    技能图标（Art/2DArt/SkillIcons/*）是带深色暗角背景的方图（alpha 恒 255），直接做广告牌会带一块
    深色底板。此处理按亮度做软阈值：luma<=8 → 全透明，luma>=32 → 保留原 alpha，中间线性过渡，
    避免硬边。这是**本工程对来源图做的唯一派生化处理**（记录在 sourcing 台账 §0）。
    """
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            luma = (r * 299 + g * 587 + b * 114) // 1000
            if luma >= 56:
                continue
            if luma <= 26:
                px[x, y] = (r, g, b, 0)
            else:
                px[x, y] = (r, g, b, int(a * (luma - 26) / 30))
    return im


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--force', action='store_true')
    args = ap.parse_args()

    ok = skip = fail = 0
    for entry in MANIFEST:
        folder, name, rel = entry[0], entry[1], entry[2]
        keyout = len(entry) > 3 and entry[3]
        out_dir = os.path.join(RES, folder)
        png = os.path.join(out_dir, name + '.png')
        meta = png + '.meta'
        if os.path.exists(png) and not args.force:
            skip += 1
            continue
        try:
            raw = fetch(CDN + rel)
            with Image.open(io.BytesIO(raw)) as im:
                im = im.convert('RGBA')
                if keyout:
                    im = keyout_background(im)
                os.makedirs(out_dir, exist_ok=True)
                im.save(png, 'PNG')
                size = im.size
            guid = load_meta_guid(meta) or uuid.uuid4().hex
            with open(meta, 'w', encoding='utf-8', newline='\n') as f:
                f.write(META.format(guid=guid))
            print('OK   %-12s %-22s %-10s %s%s' % (folder, name, '%dx%d' % size, rel,
                                                  '  [keyout]' if keyout else ''))
            ok += 1
        except Exception as e:
            print('FAIL %-12s %-22s %s  <- %s' % (folder, name, e, rel))
            fail += 1

    print('\nfetched=%d skipped=%d failed=%d' % (ok, skip, fail))
    return 1 if fail else 0


if __name__ == '__main__':
    sys.exit(main())
