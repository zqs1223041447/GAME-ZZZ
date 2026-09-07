# UNATTENDED_VERIFICATION（无人值守验证 Gate · QA 操作契约）

S3-M4 建立（Quick Gate）；S3-M5 扩展（Full Integration Gate）。本文是仓库内长期 QA 操作契约：**不需要任何聊天历史或外部工具知识，clone 仓库即可完成标准验证 Gate**。

## Canonical command

在 Unity 项目根目录（`unity/game/New Unity Project/`）执行：

```text
.\tools\verify_unattended.ps1                  # Quick Gate：测试 + Audit
.\tools\verify_unattended.ps1 -IncludeBuild    # Full Gate：Quick + StandaloneWindows64 Player Build
```

或指定编辑器路径：

```text
.\tools\verify_unattended.ps1 -UnityPath "G:\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
```

可加 `-TimeoutMinutes <n>`（默认 20，每测试平台一套件）与 `-BuildTimeoutMinutes <n>`（默认 30，Player Build 一套件）；不会无限等待 Unity。

## Quick vs Full

| | Quick | Full（`-IncludeBuild`） |
|---|---|---|
| EditMode 全套 | ✓ | ✓ |
| PlayMode 全套 | ✓ | ✓ |
| Content Audit freshness+verdict | ✓ | ✓ |
| StandaloneWindows64 Player Build | ✗（摘要明示 `PlayerBuild: SKIPPED (use -IncludeBuild)`） | ✓（win64，输出 temp） |

**什么时候要求 Full**：以下修改完成后默认要求 Full Gate——Runtime C#、Scene、ProjectSettings、Packages、Resources、build settings、player-facing asset。纯 docs / test-only / tooling-only 可由工作令明确只要求 Quick；规划 AI 有权逐轮强制 Full。

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

不启动 Unity。合成夹具验证（14 项）：PASS XML→PASS / 含失败 XML→FAIL / 缺失 XML→FAIL / 畸形 XML→FAIL / 缺 result 节点→FAIL / Audit PASS 文本→PASS / Audit FAIL 文本→FAIL / Audit 未完成文本→FAIL / freshness 纯函数 / Player 构件有效→PASS / 缺 exe→FAIL / 零字节 exe→FAIL / 缺 `_Data`→FAIL / 空 `_Data`→FAIL。全过 exit 0，任一失败 exit 非 0。

## Gate 包含什么

Unity 定位 → 项目锁检查 → EditMode 全套 →（基础设施仍允许时）PlayMode 全套 → `CONTENT_AUDIT_S3_CLOSEOUT` freshness + verdict 检查 →（`-IncludeBuild` 时）StandaloneWindows64 Player Build → XML 结果动态解析（不信任 Unity 进程退出码单独判绿，数量不硬编码）→ 统一判定。EditMode 测试红 ≠ 跳过 PlayMode 或 Build（普通测试失败仍继续收集完整证据，最终统一 FAIL）；仅基础设施崩溃（Unity 无法启动/项目无法打开/锁/测试进程基础设施失败）才跳过余下步骤并如实记录（Build 显示 NOT RUN / INFRA BLOCKED，不伪造 PASS）。

## Exit code

| 码 | 含义 |
|---|---|
| 0 | Gate PASS（全条件满足） |
| 1 | Gate FAIL（tests / audit / player build / timeout 任一必要条件失败） |
| 2 | Unity 编辑器无法解析 |
| 3 | 项目已被编辑器打开（PROJECT_ALREADY_OPEN） |

## Temp artifact location

`%TEMP%\GAME-ZZZ-UnattendedGate\`——每轮开始清理上一轮并重建：`EditModeResults.xml` / `EditMode.log` / `PlayModeResults.xml` / `PlayMode.log` / `PlayerBuild\`（Full：GAME-ZZZ.exe + GAME-ZZZ_Data）/ `PlayerBuild.log`（Full）/ `verification-summary.json`（machine-readable，ephemeral，不提交）。仓库不得出现这些产物（不靠 .gitignore 掩盖）。

## Editor-open 行为

项目被编辑器占用（`Temp/UnityLockfile` 存在）时 Gate fail fast（exit 3），**绝不杀任何 Unity 进程**。执行 AI 验收流程：正常关闭本项目编辑器 → 跑 Gate。

## Content Audit freshness 契约

Gate 记录启动时间与审计文件 LastWriteTimeUtc（前/后双记录），要求本轮 EditMode 运行确实重新持久化了 `docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md`（PASS 快照本应 byte-equivalent——hash 不变是预期，freshness 用写盘时间而非 hash 判定）；并要求文本满足 `Audit completed: YES` / `Verdict: PASS` / `Failure count: 0`。旧 PASS 快照无法骗过 Gate。

## 明确「不自动修复」

Gate 是 Reviewer 工具，不是 fixer：只记录、只判 FAIL。不改 STATUS/ROADMAP/Catalog/测试/代码，不跑任何 git 命令。唯一允许的仓库内副作用：EditMode 正式运行按既有契约再生 `CONTENT_AUDIT_S3_CLOSEOUT.md`。

## 平台

Windows · PowerShell canonical（pwsh / Windows PowerShell 兼容写法）。不要求 Bash/macOS/Linux 对等实现。
