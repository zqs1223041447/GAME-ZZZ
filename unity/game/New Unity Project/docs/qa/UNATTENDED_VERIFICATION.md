# UNATTENDED_VERIFICATION（无人值守验证 Gate · QA 操作契约）

S3-M4 建立（Quick Gate）；S3-M5 扩展（Build Gate）；S3-M6 扩展（Player Runtime Gate）。本文是仓库内长期 QA 操作契约：**不需要任何聊天历史或外部工具知识，clone 仓库即可完成标准验证 Gate**。

## Canonical command

在 Unity 项目根目录（`unity/game/New Unity Project/`）执行：

```text
.\tools\verify_unattended.ps1                    # Quick Gate：测试 + Audit
.\tools\verify_unattended.ps1 -IncludeBuild      # Build Gate：Quick + StandaloneWindows64 Player Build
.\tools\verify_unattended.ps1 -IncludePlayerRun  # Player Runtime Gate：Build Gate + 刚构建 Player 跑 Arena Harness
```

或指定编辑器路径：

```text
.\tools\verify_unattended.ps1 -UnityPath "G:\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
```

可加 `-TimeoutMinutes <n>`（默认 20，每测试平台一套件）、`-BuildTimeoutMinutes <n>`（默认 30，Player Build）、`-PlayerRunTimeoutMinutes <n>`（默认 10，Player Runtime）；不会无限等待 Unity。

## 四层 Gate

| | Quick | Build（`-IncludeBuild`） | Player Runtime（`-IncludePlayerRun`，隐含 Build） | Performance（`-IncludePerformance`，隐含全部） |
|---|---|---|---|---|
| EditMode 全套 | ✓ | ✓ | ✓ | ✓ |
| PlayMode 全套 | ✓ | ✓ | ✓ | ✓ |
| Content Audit freshness+verdict | ✓ | ✓ | ✓ | ✓ |
| StandaloneWindows64 Player Build | ✗（明示 `SKIPPED (use -IncludeBuild)`） | ✓（win64，输出 temp） | ✓ | ✓（只 Build 一次） |
| 刚构建 Player 跑 ArenaPerfHarness（100/200/300） | ✗（明示 `SKIPPED (use -IncludePlayerRun)`） | ✗（明示 `SKIPPED (use -IncludePlayerRun)`） | ✓（Player 自退 exit 0 + 三档证据解析） | → 由 Performance 层 3 次锁定环境运行取代 |
| 锁定环境 + 3 次重复运行 + 8.33ms 硬预算 | ✗（恒输出 `PerformanceVerdict: NOT_EVALUATED`） | 同左 | 同左 | ✓（唯一允许输出 `PerformanceVerdict: PASS` 的层） |

**Player Runtime Gate ≠ performance pass**：只证明「刚构建的 Player 能启动、跑完三档 Harness、产出结构化证据」。

**Performance Gate**：contract 唯一真相源=`docs/qa/PERFORMANCE_GATE.json`（预算语义文档=`docs/qa/PERFORMANCE_BUDGET.md`）。锁定环境：真实 `Screen` 分辨率 2560×1440 / fullscreen / Direct3D12 / Quality=PC / vSync=0 / targetFrameRate=-1 / CPU+GPU 锁定（probe 实测回填；硬件变更需正式工作令显式更新，禁止自动学习）。同一 Build 连续 3 次独立 Player 运行（`-screen-width/-height/-fullscreen/-screen-quality/-force-d3d12 -arenaPerf -arenaPerfGate`），全部 PASS 才 PASS；硬指标：main avg+p99 ≤8.33、cpu/gpu avg >0 且 ≤8.33、frame_timing 必须 available（否则 EVIDENCE_INCOMPLETE）、alive≥95%、frames==600。Verdict 语义（PASS/FAIL/ENV_NOT_MET/EVIDENCE_INCOMPLETE/INFRA/NOT_EVALUATED）与 R3 内容冻结契约见 PERFORMANCE_BUDGET.md；只有 Performance 层允许输出 `PerformanceVerdict: PASS`。

**什么时候要求更高层**：Runtime C#、Scene、ProjectSettings、Packages、Resources、build settings、player-facing asset 修改后默认要求 Player Runtime Gate（或更高，由工作令指定）；触及性能相关面（渲染/质量/工作负载/分辨率）默认 Performance 层；纯 docs / test-only / tooling-only 可 Quick，规划 AI 有权逐轮强制更高层。

## Player Build（Build Gate 层）

- 命令语义：`-batchmode -quit -buildTarget win64 -buildWindows64Player`（Unity 官方 CLI，实测 6000.3.23f1）。
- 输出：`%TEMP%\GAME-ZZZ-UnattendedGate\PlayerBuild\GAME-ZZZ.exe` + `GAME-ZZZ_Data\` + `PlayerBuild.log`——**全部 ephemeral，不入仓库，不靠 .gitignore 掩盖**。
- 判定双证据：Unity 进程正常完成 **且** exe 非空 **且** `<exe>_Data` 非空（exit=0 而产物无效=FAIL）；不硬编码后端产物（GameAssembly.dll 等不作判据）。
- Build Settings 场景契约（Bootstrap=0 / Arena=1，ArenaPerfHarness `LoadScene(1)` 依赖）由 `BuildSettingsContractTests` 锁定——场景顺序变更必须同步该测试与 Harness（另行工作令）。
- Player Build 产物不要求 byte-identical；Audit Markdown 保持 deterministic。

## Player Runtime（Player Runtime Gate 层）

- 只运行**本轮刚构建**的 Player（TempRoot 每轮清空，不可能吃到 stale exe）；`-IncludePlayerRun` 自动隐含 `-IncludeBuild`。
- 启动参数：`-arenaPerf -arenaPerfOut <temp>\PlayerRun -logFile <temp>\PlayerRun.log`；Player 必须自行退出且 exit=0。
- 证据契约（QA golden 钉死，不从输出枚举）：`100.txt`/`200.txt`/`300.txt` 存在于**指定**输出目录（fallback 写别处=FAIL）；每份 header density 与预期一致；`resolution=WxH`（W>0,H>0，三档一致）；`editor=False`；`dx=` 非空；恰 1 行 13 列数据；`dummy_count==密度`；`alive>0 且 ≤dummy_count`；frames>0；数值列可解析且有限；`frame_timing_ok` bool。
- 输出目录每轮启动前彻底清空（三份证据必然来自本次 Player Run）。
- Harness 结果标题保持中性（`# ArenaPerfHarness, density=<n>`），**分辨率真相只来自 `resolution=` 实测行**（`Screen.width/height`），不得由测试名/目录名/请求值推断。
- 无性能阈值、不启动性能判定；Harness 数值仅作 observation 记录（summary JSON `densities` 字段）。

## Player Build（Full Gate）

- 命令语义：`-batchmode -quit -buildTarget win64 -buildWindows64Player`（Unity Test/Build 官方 CLI，实测 6000.3.23f1）。
- 输出：`%TEMP%\GAME-ZZZ-UnattendedGate\PlayerBuild\GAME-ZZZ.exe` + `GAME-ZZZ_Data\` + `PlayerBuild.log`——**全部 ephemeral，不入仓库，不靠 .gitignore 掩盖**。
- 判定双证据：Unity 进程正常完成 **且** exe 非空 **且** `<exe>_Data` 非空（exit=0 而产物无效=FAIL）；不硬编码后端产物（GameAssembly.dll 等不作判据）。
- 不启动 Player、不做性能阈值（后续轮次另行决定）。
- Build Settings 场景契约（Bootstrap=0 / Arena=1，ArenaPerfHarness `LoadScene(1)` 依赖）由 `BuildSettingsContractTests` 锁定——场景顺序变更必须同步该测试与 Harness（另行工作令）。
- Player Build 产物不要求 byte-identical；Audit Markdown 保持 deterministic。

## Unity path 解析顺序

1. 显式 `-UnityPath`（文件不存在→立即失败）
2. 环境变量 `UNITY_EDITOR`
3. `ProjectSettings/ProjectVersion.txt` 的精确版本（当前 6000.3.23f1）
4. Unity Hub 安装根搜索（默认根 + `%APPDATA%\UnityHub\secondaryInstallPath.json` 记录的自定义根）下 `<版本>\Editor\Unity.exe`
5. 找不到：立即失败（exit 2），打印所需版本与已检查策略；**不交互、不自动下载**

## SelfTest

```text
.\tools\verify_unattended.ps1 -SelfTest
```

不启动 Unity。合成夹具验证（36 项）：XML 解析 5 项（PASS/失败/缺失/畸形/缺 result 节点）+ Audit 文本 3 项（PASS/FAIL/未完成）+ freshness 纯函数 + Player 构件 5 项（有效/缺 exe/零字节/缺 `_Data`/空 `_Data`）+ Player 证据 8 项（合法三份/缺档/密度不匹配/CSV 畸形/NaN/editor=True/alive=0/分辨率 0x0）+ Performance 15 项（contract schema/合法三轮 PASS/p99 超预算 FAIL/avg 超预算 FAIL/GPU 超预算 FAIL/分辨率不符 ENV_NOT_MET/API 不符 ENV_NOT_MET/Quality 不符 ENV_NOT_MET/vSync 不符 ENV_NOT_MET/targetFps 不符 ENV_NOT_MET/硬件不符 ENV_NOT_MET/FrameTiming 不可用 EVIDENCE_INCOMPLETE/alive 比例无效 FAIL/frames 无效 FAIL/文件级硬件与环境行捕获）。全过 exit 0，任一失败 exit 非 0。

## Gate 包含什么

Unity 定位 → 项目锁检查 → EditMode 全套 →（基础设施仍允许时）PlayMode 全套 → `CONTENT_AUDIT_S3_CLOSEOUT` freshness + verdict 检查 →（`-IncludeBuild` 时）StandaloneWindows64 Player Build →（`-IncludePlayerRun` 时）启动本轮刚构建 Player 跑 `-arenaPerf` 并解析三档证据 → XML/证据动态解析（不信任进程退出码单独判绿，数量与密度档不硬编码为成功条件）→ 统一判定。测试红 ≠ 跳过后续步骤（普通测试失败仍继续收集完整证据，最终统一 FAIL）；仅基础设施崩溃才跳过余下步骤并如实记录（Build/PlayerRun 显示 NOT RUN / INFRA BLOCKED，不伪造 PASS）。

## Exit code

| 码 | 含义 |
|---|---|
| 0 | Gate PASS（全条件满足） |
| 1 | Gate FAIL（tests / audit / player build / timeout 任一必要条件失败） |
| 2 | Unity 编辑器无法解析 |
| 3 | 项目已被编辑器打开（PROJECT_ALREADY_OPEN） |

## Temp artifact location

`%TEMP%\GAME-ZZZ-UnattendedGate\`——每轮开始清理上一轮并重建：`EditModeResults.xml` / `EditMode.log` / `PlayModeResults.xml` / `PlayMode.log` / `PlayerBuild\`（GAME-ZZZ.exe + GAME-ZZZ_Data）/ `PlayerBuild.log` / `PlayerRun\`（100/200/300.txt）/ `PlayerRun.log` / `Performance\Run1-3\`（每轮独立清空：100/200/300.txt + PlayerRun.log）/ `verification-summary.json`（machine-readable，ephemeral，不提交）。仓库不得出现这些产物（不靠 .gitignore 掩盖）。

## Editor-open 行为

项目被编辑器占用（`Temp/UnityLockfile` 存在）时 Gate fail fast（exit 3），**绝不杀任何 Unity 进程**。执行 AI 验收流程：正常关闭本项目编辑器 → 跑 Gate。

## Content Audit freshness 契约

Gate 记录启动时间与审计文件 LastWriteTimeUtc（前/后双记录），要求本轮 EditMode 运行确实重新持久化了 `docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md`（PASS 快照本应 byte-equivalent——hash 不变是预期，freshness 用写盘时间而非 hash 判定）；并要求文本满足 `Audit completed: YES` / `Verdict: PASS` / `Failure count: 0`。旧 PASS 快照无法骗过 Gate。

## 明确「不自动修复」

Gate 是 Reviewer 工具，不是 fixer：只记录、只判 FAIL。不改 STATUS/ROADMAP/Catalog/测试/代码，不跑任何 git 命令。唯一允许的仓库内副作用：EditMode 正式运行按既有契约再生 `CONTENT_AUDIT_S3_CLOSEOUT.md`。

## 平台

Windows · PowerShell canonical（pwsh / Windows PowerShell 兼容写法）。不要求 Bash/macOS/Linux 对等实现。
