# S6P-WO-05 — Passive Tree Overview LOD & Texture Residency

**状态：** IMPLEMENTING  
**合同：** `S6P_WO_05_CONTRACT.md`（Channel A，69 AC）  
**入口 HEAD：** `ef11cae`（04C CLOSED）  
**ProdSim：** 必须保持 `FNV1A64:99f1bfd3f81c4fe6`

## 已落地

- LOD 权威纯函数 `PassiveTreeLod`：`NormalProjectedPx = 46 * zoom * DesignScale`
- 切档：&lt;8 Overview / [8,18) Mid / ≥18 Detail；边界 7.999/8/17.999/18 有测试
- `PassiveTreeRenderPlan`：可见集不吃 LOD；LOD0 不请求 individual icon / frame / group
- `PassiveTreeTextures`：树视觉纹理 owner；Sync(plan) 后 LOD0 resident icons=0
- `HitNodeId`：物理半径、平局取小 NodeId；fixture 2172/71/183/10/1006/keystone/jewel × 三档 5 probe PASS
- `SliceHud` 按 plan 绘制：LOD0/1 简化符号，Detail 才加载完整图标与名字

## 本轮又落地

- 2429 centre probe mismatch=0；duplicate-position 组 winner 确定
- 九宫 pan sweep：resident icons == required stems，无历史累积
- LOD2→LOD0：individual icons 与 chrome owner 归零
- 路径 [2172,71,183] 三档 NodeState/Effect/Traversal 不变
- Headless Chrome 对拍 G0–G3 × 1920/2560：setMismatch=0，max centre error ≤ 0.0038 px（≤1 px）

## 未做

120 帧稳定采样、PlayMode 截图、edge-endpoint 对拍表、CLOSED 关树后的实机释放时序。
