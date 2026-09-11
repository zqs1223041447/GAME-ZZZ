# S6P-WO-03 Rework Delta — AC-22 UI Mechanical Gate

**前置 Gate：** REJECT-REWORK（AC-22 FAIL；其余 36 PASS）  
**本 delta：** 只修 AC-22 + AC-30 要求的 selector integration flow。  
**ProdSim：** 未改 serializer / 未改 gameplay hash。`FNV1A64:99f1bfd3f81c4fe6` 仍是上一轮冷进程结果，本 delta 不请求晋升。

## 修了什么

1. **单一几何权威** `SliceHud.BuildMasterySelectorLayout(dw, dh, nodeId)`。`DrawMasterySelector` 只绘制这份 layout，不再另写 48/22 常量。
2. **长文本包含**：行高 = `EstimateWrappedHeight`（纯函数，word-wrap，不碰 `GUI.skin`）+ 状态行。绘制用 `wordWrap=true`、`TextClipping.Overflow`。测试断言每条官方 choice 的 text rect 高度 ≥ 换行高度。
3. **去掉恒真比较**：`Assert.AreEqual(sourceCount, layout.Rows.Length)`，source = `PassiveSupport.ChoiceCount`。
4. **open/cancel 零 mutation**：`SliceHud.OpenMasterySelector` / `CancelMasterySelector` 不碰 session。EditMode + PlayMode 都有。
5. PlayMode 新增 `Mastery_SelectorOpenCancelCommitReset_Integration`。

## 门

| 项 | 值 |
|---|---|
| EditMode | **490/490** |
| PlayMode | **20/20** |
| ProdSim hash | 未重跑冷进程（本 delta 无 gameplay/serializer 变化） |

## 请 Channel A

最小 Gate Review：AC-22 是否 PASS。若 PASS 且无新阻塞，请 ACCEPT 并放行 WO-04B。hash 是否正式晋升为 V3 baseline 请一并裁定。
