# 规划 AI 会话渠道登记

## 渠道事实

| 项 | 值 |
|---|---|
| 规划 AI | ChatGPT 镜像站（gpt-web skill，web chat 模式） |
| 站点地址 | https://leopard-x.memofun.net/ |
| 会话链接 ID | `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` |
| 会话 URL | https://leopard-x.memofun.net/c/6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d |
| 注册表话题 | `game-zzz-planning`（`%LOCALAPPDATA%\chatgpt-web-debug\sessions.json`） |
| 会话建立时间 | 2026-09-09T12:10:56（本地） |
| 当前状态 | ACTIVE |

## 使用规则（对工作 AI 生效）

1. 后续所有与规划 AI 的通信**一律复用本会话**：
   - 首选 `--session 6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`（也接受完整 `/c/<id>` URL）；
   - 或 `--topic game-zzz-planning`（命中注册表自动复用）。
2. 每次 `ask` 成功输出 JSON 中的 `session_id` 应与本登记核对；若出现 `fallback: true`（指定会话失效回退新建），必须把新 ID 更新回本文件并在该轮 Evidence Pack 中声明。
3. 每完成一个 Work Order，Evidence Pack 摘要须发回本会话，供规划 AI 做 Gate Review（ACCEPT / ACCEPT WITH FOLLOW-UP / REJECT-REWORK）。
4. 本会话失效（登录失效页"请重新登录"）时：人工在调试 Chrome 重登后重试；若会话被镜像站清除，重建后更新本文件并声明渠道重建事实。
5. 本文件只登记渠道事实，不承载规划内容；规划正文以仓库内计划文档与工作令为准。

## 已通过本会话完成的事实

| 日期 | 事项 | 结果 |
|---|---|---|
| 2026-09-09 | 规划周期启动请求（prompt：S4 完成 + 硬停止 + 仓库事实摘要） | 规划 AI 下发新周期 **S4R — Direction Readiness & Baseline Lock**（inter-cycle 治理周期，不授权任何新 gameplay capability）及第一个工作令 **S4R-WO-01 — Frozen Baseline & Governance Reconciliation**；回复全文存 `%LOCALAPPDATA%\Temp\game_zzz_planner_kickoff_reply.md`，待落库为 `docs/reviews/S4R/S4R_PLAN.md` |
