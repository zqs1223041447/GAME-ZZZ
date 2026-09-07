# S3_M6_PLAYER_RUNTIME_TRUTH_REVIEW（S3 维护轮 M6 独立复核）

日期：2026-09-08。工作令：`S3-M6-PLAYER-RUNTIME-TRUTH`。Phase R（A15 Reviewer）：以真实 diff + 实际运行复核。Baseline HEAD=c9ca5d7。

## Baseline HEAD / Before verification model

- Baseline HEAD=c9ca5d7；Quick Gate（测试+Audit）与 Build Gate（+win64 build）已就位。
- Before：仓库能证明「当前 HEAD 能构建出 Windows Player」，但**不能证明「刚构建的 Player 能启动并完整执行 Runtime Harness」**；另有两处证据语义缺陷：Harness 结果首行固定写 `# S2P 1440p harness`（历史实测实为 1920×1080）、metadata 含 `enteredMap=` 写死常量（假证据）。

## Player Runtime command / Built Player identity

- `.\tools\verify_unattended.ps1 -IncludePlayerRun`（**自动隐含 -IncludeBuild**，无需同时写两个 switch；同时提供亦正常——代码实现于参数绑定后立即合并）。
- Built Player：`%TEMP%\GAME-ZZZ-UnattendedGate\PlayerBuild\GAME-ZZZ.exe`——TempRoot 每轮开始清空重建，**不可能吃到 stale Player**（工作令七从源头保证）。
- 三层兼容：Quick 与 Build Gate 行为与 M4/M5 完全一致（未偷偷增加 Player 启动时间；-IncludeBuild 单独使用时不运行 Player）。

## Player launch args / timeout / process result

- 启动参数：`-arenaPerf -arenaPerfOut "<TempRoot>\PlayerRun" -logFile "<TempRoot>\PlayerRun.log"`（实测 Unity 6000.3.23f1 Standalone 兼容）。
- Timeout：`-PlayerRunTimeoutMinutes` 默认 10；`Start-Process -PassThru` 只持自己启动的 Player child；超时 Kill 自己 child→TIMEOUT→Gate FAIL；无任何广域 Stop-Process。
- Process result：两次 Player Runtime Gate 运行 **Player 均自行退出，Exit Code=0**（三档写完后 `Application.Quit()`），无需人工关窗口。

## Output result / Harness parser contract

- 输出目录每轮启动前彻底清空重建 → 100/200/300 三份证据必然来自本次 Player Run（工作令十八从源头保证，不靠文件日期猜测）。
- Parser 契约（纯函数 `Read-HarnessResult`/`Test-PlayerRunEvidence`）：文件存在于**指定** temp 输出目录（fallback 写到别处=FAIL，工作令三十八）→ 首行 header density 与预期一致 → metadata `resolution=WxH`（W>0,H>0）、`editor=False`（True 即 FAIL）、`dx=` 非空 → CSV header 存在 → 恰 1 行数据 13 列 → dummy_count 整数==预期密度 → alive>0 且 ≤dummy_count → frames>0 → 8 个数值列可解析且有限（NaN/Infinity 拒绝）→ frame_timing_ok bool → 三档 resolution 一致（不一致=evidence unstable FAIL）。
- 密度预期为 **QA golden 钉死 @(100,200,300)**（不从结果目录枚举反推）；未来改密度档位必须同步 Harness 与 QA contract。
- 无任何性能阈值：数值只解析合法性并记录，不做优劣判定。

## Run 1 / Run 2（实测）

| Run | Exit | EditMode | PlayMode | Audit | Build | PlayerRun |
|---|---|---|---|---|---|---|
| 1 | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64（667136B/162 files） | PASS exit=0, densities=100/200/300 |
| 2 | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64 | PASS exit=0, densities=100/200/300 |

- 证据示例（Run 1, 300 档尾部日志）：`harness wrote ...\PlayerRun\300.txt`、`harness 300 alive=290 avg=1.656`——300 档 alive=290/300 为当前合法 Harness 行为（契约 alive≤dummy_count，不要求相等）。
- PlayerRun.log 保存完整（Engine 启动→三档写出→Player 退出）；`[Perf] harness wrote` 作辅助证据，核心判据=进程+结构化结果文件。

## Actual resolution / Graphics API / Performance verdict

- **actual resolution=2560x1440**（`Screen.width/height` 实测，三档一致）——仅记录观察，**不构成任何 1440p 结案或性能判定**。
- Graphics API：Direct3D12（观察记录；M6 不新建 GPU API 门，项目现有 DX12 契约保持）。
- **Performance verdict = NOT_EVALUATED**（控制台每次明示）；**1440p/120 = NOT CLOSED（未结案）**。

## Evidence-title fix / enteredMap audit（Runtime 唯一改动）

- `ArenaPerfHarness.WriteRow`：首行 `# S2P 1440p harness, density=n` → **中性标题 `# ArenaPerfHarness, density=n`**（不再声明未经验证的分辨率；实际分辨率由 `resolution=` 实测行独立承载）。
- `enteredMap=`（硬编码常量 `isEditor?n/a:false`，无测量含义的假证据）→ **删除**（工作令二十四最小处置）。
- **行为零变化**：Densities/WarmupFrames/SampleFrames/CastInterval/Spawn strategy/Skill rotation/PerfSampler/Combat 全部未动（diff 复核：仅 2 行证据元数据）。

## 历史 S2P 文件冻结

- `docs/reviews/s2p/`（1440p/100|200|300.txt、player-run.log 等）**零改动**（git 复核）；历史首行「1440p harness」保留原样。
- 按 S3-M6 授权在 `S2P_CLOSEOUT.md` 补一句消歧注（历史标题 vs `resolution=` 实测为准），不改任何历史数据文件。

## SelfTest（parser 失败路径全覆盖）

22/22 PASS，exit 0（原 14 项 + 新 8 项 Player 证据夹具：pass / missing-density / density-mismatch / malformed-csv / invalid-number(NaN) / editor=true / zero-alive / invalid-resolution(0x0)）。夹具全部系统 temp 创建，运行后清理，不入仓库（工作令二十八）。

## Repository pollution

- Player 证据与日志全在 `%TEMP%\GAME-ZZZ-UnattendedGate\PlayerRun\`（100/200/300.txt + PlayerRun.log）；不写 docs/reviews/s2p/、不产生仓库内 Logs/。
- 两次 Gate 后 git：仅剩本轮工作文件（harness 2 行 + gate 脚本 + docs）；零 PlayerRun 产物污染。

## Runtime delta / Content delta

- Runtime 修改=仅 `ArenaPerfHarness.cs` 证据元数据 2 行（标题中性化+删假字段）；gameplay 行为 delta=0。
- Content delta=0（全轴）；Phase 3-5 未启动；未做性能优化（观察到三档 avg 1.351/1.453/1.656 ms 仅作 observation 记录，不设阈值、不触发优化）。

## Reviewer 必答 Q1-Q8

- Q1 Player 能 build 但启动即崩 → Gate 红？**YES**（exit≠0/timeout→PlayerRun FAIL→Gate FAIL）。
- Q2 Player 启动但 Harness 没跑 → 红？**YES**（指定输出目录缺文件→evidence invalid）。
- Q3 只写 100/200 没有 300 → 红？**YES**（missing density；SelfTest 夹具证明）。
- Q4 文件来自上次运行能骗过吗？**NO**（输出目录每轮彻底清空重建）。
- Q5 文件写 1440p 标题但实际 1080p 的情况还会在新证据发生吗？**NO**（标题中性化+actual resolution 单独实测）。
- Q6 100/200/300 性能很差会误判 Performance PASS 吗？**NO**（无阈值；PerformanceVerdict: NOT_EVALUATED 恒输出）。
- Q7 Player hang 会永久卡住无人值守 AI 吗？**NO**（-PlayerRunTimeoutMinutes，只杀自己 child）。
- Q8 历史 S2P 数据会被本轮覆盖吗？**NO**（输出全在 temp；s2p 目录 git 零改动实证）。

## Remaining risks

- 低：`SceneManager.LoadScene(1)` 场景索引契约（M5 已测试锁定；Player Runtime 依赖同一契约，场景结构变更须同步 Harness+契约测试）。
- 低：Player 分辨率为显示器原生实测值（当前 2560×1440；历史 S2P 轮为 1920×1080）——两轮环境不同不构成可比性结论；性能环境标准化留给未来 Performance Gate 轮。
- 低：Harness fallback（`-arenaPerfOut` 不可用→写项目 Logs）保留（手工游玩可用性）；Gate 侧通过「指定目录缺文件即 FAIL」防御（工作令三十八）。
- 低：Player 无图形环境标准化（分辨率/Fullscreen/vSync 均未强制）——工作令九明确留给 M7+。

## Verdict

**PASS**
