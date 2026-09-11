# S6P-WO-05 — Passive Tree Overview LOD & Texture Residency

**状态：** IMPLEMENTING  
**合同：** `S6P_WO_05_CONTRACT.md`（Channel A，69 AC）  
**入口 HEAD：** `ef11cae`（04C CLOSED）  
**ProdSim：** 必须保持 `FNV1A64:99f1bfd3f81c4fe6`

## 已落地

- LOD 权威纯函数 `PassiveTreeLod`：`NormalProjectedPx = 46 * zoom * DesignScale`
- 切档：&lt;8 Overview / [8,18) Mid / ≥18 Detail；边界 7.999/8/17.999/18 有测试
- 同投影像素在 1080p 与 1440p 同档、zoom 不同（禁止 raw-zoom LOD）
- Full-icon / group-decoration 策略函数
- HitRadius 6px 下限

## 未做

Render plan、renderer 接线、texture residency owner、HitNodeId、Chrome 几何对拍、显存 Gate。
