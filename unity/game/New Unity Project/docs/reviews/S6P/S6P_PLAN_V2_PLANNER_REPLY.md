结论：接下来最该做的三件事
第一，照做 S6P-WO-02，但必须把它定性为“受支持被动子集进入确定性真相”，而不是“天赋域已经被 ProdSim 全覆盖”。 这张令现在值得做，因为当前 ProdSim 对天赋变化完全失明；先建立 canonical passive identity，后面 WO-03/04 的行为修正才能有一个稳定的“游戏真相序列化”骨架。关键是不要在测试层重新实现一套 modifier 解释器：测试只能读取运行时已经计算出的被动分配、有效 modifier 和最终属性，再做 canonical 化。历史代码本来也是 ProductionSimulator 驱动真实 
SliceSession 路径，而不是另造模拟运行时。GitHub+1
第二，把原定 WO-04 提到 WO-03 前面，先彻底消灭“花点买假效果”。 2005/2429 可点但无法完整兑现，是比“专精默认第一条”更基础的真相破损。尤其是一个节点如果同时含 
CONSUMED 和 
BLOCKED_BY_DOMAIN，不能只吃掉能识别的半截然后声称这个节点支持；当前口径下应该整节点拒绝分配。否则所谓“100% 真生效 / 0% 假生效”根本没有成立。
第三，在 LOD/显存之前插一个“Build Identity / Snapshot / Lock”骨架令。 2429 节点加专精选择，已经不是最初小树的状态量级了。所有 build snapshot、进图锁定、重构、clone/序列化以及 ProdSim canonical projection，都必须证明能无损表达这个状态。这个问题看起来不像功能，却比 LOD 更像真正的骨架。旧公开快照甚至曾用一个 
int PassiveMask 表达被动状态；我不认为本地现在一定还这样，但这足以要求对所有旧状态边界做一次机械审计。GitHub+1
因此我给出的实际执行顺序是：WO-02 → WO-04A → WO-03 → WO-04B → WO-05。编号不按数字顺序，是因为我建议把已经排好的 WO-04 拆成“运行时真相”和“Build Identity”两个阶段。

工作令执行清单
S6P-WO-02 — Passive-Aware Canonical Production Simulation
状态：维持已授权，立即执行。
为什么现在做
现在 ProdSim 的哈希对整个被动域失明，而后续所有正确性修改都会继续扩大这个盲区。先建立 canonical truth projection，可以让后续行为修改拥有稳定的“不该变就绝不能变”的基准。
但它只能证明：

当前被明确支持、非专精、非 unsupported 的一组被动状态，已经进入确定性门禁。

不能写成“ProdSim 已覆盖整个天赋域”。
改动面
域层：允许增加一个只读 canonical projection seam，前提是直接消费现有运行时最终真相，不重新解释 tooltip/effect text。
绘制层：0。
测试：ProductionSimulator、canonical serializer、敏感性契约、顺序无关契约、三进程重复执行工具。
资产：0。
可机械判定的完成口径

固定 fixture 中所有 NodeId 都作为常量写进契约，测试启动时逐个验证：

存在于内嵌官方数据；

属于 2429 个可上树节点；

非 mastery；

非 
UNSUPPORTED_CURRENTLY；

从合法起点可通过现有 API 合法分配；

至少有一条现有引擎真正消费的进攻结果；

至少有一条现有引擎真正消费的防御或属性结果。

fixture 的发现过程可以由工具一次性从源数据推导并留下 provenance，但最终测试不得运行时动态选节点。

canonical 输入至少含：

已分配 NodeId 集合；

运行时最终生效 modifier 的语义多重集；

运行时最终生效结果，其中必须同时观察到固定进攻上下文和固定防御/属性上下文。

canonical 化必须：

NodeId 明确排序；

modifier 用稳定的语义字段排序，保留重复项 multiplicity；

数值用 invariant、无显示格式歧义的编码；

不读取字典枚举顺序。

明确断言以下字段不进入哈希：坐标、zoom、pan、atlas、UV、icon path、tooltip、localization、GUI 状态、整树原始字节。

敏感性 A：合法改变分配集合，哈希不同。

敏感性 B：合法切换到另一组导致实际最终数值不同的支持节点，哈希不同。

敏感性 C：恢复原状态，哈希逐 bit 精确复原。

同一最终集合用不同枚举顺序输入 canonicalizer，哈希相同；如果使用两条分配序列，则两条序列自身也必须都满足合法加点规则。

Run1 / Run2 / Run3 必须是三个独立 Unity batch process，各自从干净进程启动、生成独立证据文件，再比较三者。不能把同一个测试进程中连续调用三次 
RunCore() 冒充三次独立运行。旧公开实现本身就是单次调用里重复跑核心逻辑，因此这一点尤其需要钉死。GitHub+1

Run1 == Run2 == Run3、
invalid=0。

生成一个新的 FNV1A64 canonical hash；旧 
9a4c9524d0b3e214 只写 predecessor metadata。

全量 EditMode、PlayMode 继续零失败。不要继续把 
394/394、
14/14 写成未来固定总数，因为新增测试后分母理应增长。

取证门禁
EditMode：canonicalization、合法 fixture、A/B/C、顺序无关。
PlayMode：如果最终属性只有完整运行会话才能证明，则用真实 session 做至少一个消费链契约。
ProdSim：三个独立 batch invocation + 新 hash + 
invalid=0。
最容易翻车 / 止损线
最大风险是为了拿到“effective modifiers/results”，在 ProductionSimulator 里复制一套 parser / Stat 解释逻辑。一旦出现第二套 passive effect evaluator，立即停工。 历史 tooling 约定本身就强调 collector/oracle 单一来源，避免平行扫描器和平行真相。GitHub+1
如果现有数据中找不到满足全部硬约束的冻结 fixture，就输出 
UNRESOLVED_WITH_SOURCE 并让 WO-02 红灯；禁止动态放宽到 mastery、unsupported 或绕过合法分配 API。

S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination
状态：我建议现在授权，并提前到 WO-03 前执行。
为什么现在做
这是当前最大的产品语义漏洞：2005 个节点允许消费点数，但不能完整兑现自身文本承诺。
专精只是其中一个特殊错误；如果先把专精 UI 做漂亮，却没有统一 support truth，用户只会从“自动拿到一个假效果”升级成“自己选了一个假效果”。
改动面
域层：建立唯一的被动支持性真相；修改分配入口，使 unsupported 在状态变更之前原子拒绝。
绘制层：只消费域层的 support state，禁止 GUI 自己重新判断 tooltip。
测试：全树 partition 契约、supported/unsupported 原子分配契约、mixed-effect 契约、负向 PlayMode。
资产：0。
可机械判定的完成口径

2429 个上树节点全部获得唯一且来源可追溯的运行时状态；不得存在 UNKNOWN。

支持定义固定为：

所有实际 gameplay effect 均有现有引擎的 
CONSUMED 处理；

STRUCTURAL 本身不构成 unsupported；

BLOCKED_BY_DOMAIN 必然导致该节点当前不可分配；

SPECIAL_INTERACTION 若找不到现有运行时 handler 的明确证据，也按当前不支持处理。

一个节点只要存在一条无法兑现的 gameplay effect，整节点拒绝。不得出现“半个节点偷偷生效”。

unsupported 节点走公开合法分配 API 时：

返回明确失败；

未花点；

allocated set 不变；

modifier set 不变；

effective results 不变。

supported 节点继续可以合法分配，并至少有契约证明其声明的当前支持效果进入真实消费路径。

mastery 在 WO-03 完成前不得继续“默认第一条生效”。这一阶段可以将 mastery 明确标成 pending/unavailable，使其不能消费点数；不要临时发明另一套默认规则。

UI 的“能不能点”必须读取同一个 domain truth；不得维护第二张 
unsupportedNodeIds GUI 表。

403 个无坐标、官方不绘制节点继续存在于 census/source accounting 中，但绝不进入玩家可点节点的失败门槛。

70 个未授权 Stat/玩法轴继续只是 dependency report，不作为本令要求实现的 blocker。

WO-02 canonical fixture 因为明确不含 unsupported/mastery，执行本令后其 canonical hash 应保持不变。如果变了，视为意外真相漂移，先查清再继续。

取证门禁
EditMode：2429 全量 partition、UNKNOWN=0、mixed node、原子拒绝、单一 truth source。
PlayMode：真实 UI/API 点击一个 frozen unsupported 节点，证明点数和 stats 零变化；再点一个 frozen supported 节点作正对照。
ProdSim：WO-02 新 hash 精确不变。
最容易翻车 / 止损线
最危险的是把“unsupported”理解为“这个 effect 跳过，但节点里别的 effect 继续吃”。那会重新制造假承诺。
另一条止损线是：禁止为了把红节点变绿而新增 StatId、补新玩法 handler。 看到冰冷、ES、格挡、召唤物、异常状态等缺轴，只报告依赖；不得实现。

S6P-WO-03 — Mastery Explicit Selection Correctness
状态：授权，但在 WO-04A 之后执行。
为什么现在做
WO-04A 先解决“这个 option 到底有没有能力兑现”，WO-03 再解决“玩家明确选择哪一个 option”。这样 mastery selector 不需要自己建立另一套支持性逻辑。
改动面
域层：mastery selection state、合法性验证、选项应用和撤销。
绘制层：显式选择交互；只能展示/禁用由 domain truth 返回的状态。
测试：无默认、原子选择、切换、重构、非法 option。
资产：原则上 0，复用现有 UI/Aria；无购买。
可机械判定的完成口径

不存在任何 
first option / index 0 自动生效路径。

mastery modifier 生效的必要条件是同时具备：

mastery node 合法；

明确的 selected option identity；

该 option 当前完全受支持。

未选择 option 时不得静默产生 modifier。

当前完全不支持的 option 不得被提交。

若一个 mastery 当前没有任何 fully-supported option，则整个 mastery 当前不可完成分配，不消费点数。

提交必须保持原子性：非法 node/option、无点数、断连等任一验证失败后，build state 完全不变。

同一个 mastery 在两个合法选项间切换时：

selected option identity 变化；

对应 modifier/effective result 正确变化；

切回原 option 后结果精确恢复。

R 重构后 node allocation 与 selected option 一并清理。

GUI 不得以数组顺序表示选择身份；必须存稳定 option identity。

WO-02 主 canonical fixture 明确 mastery=0，所以本令不应为了“让总 hash 看见 mastery”去修改那个 fixture。Mastery 自身用独立 EditMode/PlayMode 契约证明即可。

取证门禁
EditMode：selection state machine、无默认、合法/非法 option、切换/恢复。
PlayMode：真实点击流程，验证选择前后实际数值。
ProdSim：WO-02 canonical hash继续不变。
可选的无头截图：只用于证明 selector 不发生布局退化；绝不能替代行为测试。
最容易翻车 / 止损线
不要把 tooltip 数组下标当永久 option identity。上游数据一旦重排，build 会在无报错情况下选到另一个效果。
若源数据无法证明 option 的稳定身份关系，就写 
UNRESOLVED_WITH_SOURCE；禁止用文本排序、出现顺序或本地化字符串猜 ID。

S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity
状态：新增骨架令；授权，排在表现优化之前。
为什么现在做
S5U 把状态空间从小型原型树扩展成 2429 节点；WO-03 又新增 mastery option identity。此时不检查所有 build boundary，后面极容易出现：

UI 看着对；

当前 session 数值也对；

一进地图、clone build、reset、snapshot 或序列化就丢状态。

旧公开实现曾把被动 build identity 压在 
PassiveMask 一类紧凑字段里，所以这不是理论风险。这里引用的是历史结构风险，不是断言你本地仍存在该字段。GitHub+1
改动面
域层：现有 build identity、snapshot、map-lock、clone/round-trip seam。
绘制层：0。
测试：状态全保真、lock、respec、现有 serialization seam。
资产：0。
可机械判定的完成口径

对本地代码机械扫描所有与 passive build identity 有关的：

bit mask；

固定位宽整数；

fixed-size array；

snapshot；

build lock；

clone/copy；

serializer；

map-entry capture；

reset/respec。

扫描结果必须逐项归类：

SAFE_CURRENT;

MIGRATED;

NOT_APPLICABLE_WITH_SOURCE；

UNRESOLVED_WITH_SOURCE。

不允许存在“只能表示前 32/64 个节点”之类遗留状态容器。

冻结一个合法 build fixture，含多个 supported passive；如果当前已有至少一个 fully-supported mastery，则另加 mastery selection identity。

live → snapshot 后，canonical allocation identity 精确相等。

如果工程当前存在 clone/serialize/deserialize seam，则 round-trip 后 identity 和 effective truth 精确相等。

如果工程根本没有 save/persistence，不得因为本令顺手发明存档系统；记录 N/A 来源即可。

进入现有 build-lock 状态后，allocate/respec/mastery change 全部通过真实 API 被拒绝，且 snapshot/live 真相不漂移。

解锁/离开相应状态后行为恢复正常。

respec 后 allocation 与 mastery choice 均不存在残留。

所有集合 canonicalization 不依赖字典/HashSet 枚举顺序。

取证门禁
EditMode：snapshot/round-trip/clone/lock contract。
PlayMode：如果 map entry/build lock 只在完整 session 存在，则走真实流程证明。
ProdSim：canonical hash不得因 snapshot 实现细节改变。
最容易翻车 / 止损线
不要把这个 WO 演变成“顺便设计账号存档”。只治理工程已经存在的状态边界。
发现旧紧凑状态字段时也不要破坏性迁移；先增加契约、提供兼容转换，再移除旧路径。禁止 destructive git。

S6P-WO-05 — Passive Overview LOD & Icon Residency Governance
状态：最后做。
为什么现在做
到这一步，什么是节点、什么能点、mastery 怎么表示、build identity 如何保存，都已经稳定。此时才值得优化绘制。
否则过早做 LOD，很容易把“为了少画东西”误写成“这个东西不存在/不能交互”，最后把表现层变成新的游戏真相来源。
改动面
域层：原则上 0。
绘制层：zoom LOD、可见性计算、draw-call/icon presentation、tooltip/detail activation。
测试：LOD band、render population、交互与 LOD 解耦、重复开关树资源稳定性。
资产：只治理现有 750 icons / 6 atlases；不采购、不替换框架。
可机械判定的完成口径

LOD band 的阈值是显式、固定、可单测的数据，不散落成 OnGUI magic numbers。

同一 camera/zoom 输入必然得到同一 LOD。

远景可降低 icon/text/detail 绘制量，但不能修改：

allocation set；

hit-test 的节点身份；

modifier/effective result；

canonical gameplay hash。

用 frozen camera states 对至少近/中/远三个 zoom 档做自动截图基线。

近景最终图与当前正确视图做像素/规定容差对拍，不依赖人工判断。

scripted pan + zoom 后，指定 NodeId 的交互身份保持一致；不得因为 icon 被 LOD 隐藏而点击成邻居。

Passive icon registry 中不存在每节点复制 
Texture2D 的行为；750 icon 必须继续引用 canonical atlas/sprite source。

连续打开/关闭/缩放/平移树若干固定轮次后，passive atlas/resource 数量不得单调增长。

如果当前六图集全部常驻并且不存在重复/leak，不得为了“显存治理”擅自造复杂 streaming 系统。

只有机械 profiling 已证明 residency 本身越过现有预算，才允许增加 atlas residency policy。

WO-02 canonical FNV1A64 必须逐字节保持不变。一张纯表现令改变游戏 hash 就直接判失败。

取证门禁
EditMode：LOD/culling/residency policy。
PlayMode：固定 camera trajectory、资源对象计数、交互一致性。
无头截图对拍：固定近/中/远视图。
ProdSim：hash 不变。
最容易翻车 / 止损线
不要给“显存优化”先拍脑袋定一个跨机器统一的 VRAM MB 数；驱动、Unity backend 都可能制造噪声。优先门禁确定性的结构量：atlas 集合、重复纹理数、对象增长量、draw population。只有已有锁定性能环境时再把显存 MB 做硬门。

对现有 WO-02 合同与排序的直接质疑
1. WO-02 本身该做，但“门禁真正覆盖天赋域”这个说法太大
它的 canonical fixture 明确要求：

mastery = 0；

unsupported = 0。

所以即使这张令 100% 绿，也只能证明：
“支持的普通被动子集已经进入 ProdSim。”
它仍然对 315 mastery 的选择正确性和 2005 unsupported 的产品语义故意失明。
这没有问题；问题只在于不能拿一个绿 hash 当整个 Passive Truth 的结业证明。
2. 我不同意 WO-03 → WO-04 的原排序
我会改成：
WO-04A support truth → WO-03 mastery selection。
理由很简单：先知道一个效果能不能兑现，再让玩家选择它。
否则 WO-03 必须自己回答“这个 mastery option 能不能选”，随后 WO-04 又再回答一次，最后大概率出现两个 oracle。
3. WO-02 的“三次独立重跑”合同还要更硬一点
“三次真正独立”应直接定义为三个独立 Unity 进程，不是一次 Unity invocation 中连续执行三遍，也不是一个 test process 内清几个 static field 后重跑。
旧公开 ProdSim 就曾在同一次测试入口内做重复运行，因此如果合同不写成 process-level isolation，很容易得到一个形式上满足、实际上没有覆盖静态缓存/初始化顺序问题的证明。GitHub+1
4. “modifier + effective result 都进 hash”是对的，但不能变成重复真相
这里最值得防的是：
passive source text -> TestParser -> hash
同时运行时又是：
passive source text -> RuntimeParser -> StatBag
这样 hash 会很稳定，却稳定地证明了测试自己的实现。
正确关系必须是：
source → 唯一 runtime truth → canonical projection → FNV1A64
ProductionSimulator 只负责编码真相，不负责重新判断真相。历史工程的 tooling 方向也是复用既有 collector/oracle，而不是再建一套 scanner。GitHub

你没提、但我认为需要提前钉死的风险
一是“进攻被动”和“全局属性被动”可能走两条实际消费链。 历史运行时就曾存在全局 
RecalcPlayer 与技能上下文 modifier 收集的分流。也就是说，只验证“modifier list 变了”还不够：WO-02 必须证明一个进攻节点最终改变了真实技能上下文数值，同时另一个防御/属性节点改变了真实 player result。否则会出现“hash 看到了 modifier，但游戏那条消费链没吃到”的假绿。GitHub+1
二是 mixed node 是最隐蔽的覆盖率陷阱。 一个节点五条词条里四条 
CONSUMED、一条缺轴，绝不能统计成“这个节点部分支持”然后允许玩家购买。以你现在确定的产品口径，它整个就是 unsupported。否则 21.9%、607 行这类数字很容易不知不觉重新变成 KPI。
三是 canonical serializer 自己会成为新的核心协议。 NodeId 排序还容易；modifier 更危险。必须明确稳定 key、重复项处理、浮点编码、条件/tag identity。不能靠 
ToString()、enum 显示名、localized text 或当前字典顺序，否则一次无害重构就会制造 hash migration。
四是上游 PoE 数据版本漂移。 Fixture NodeId 要冻结，但同时应该在 review 证据中固定其来源数据版本/checksum。这个 checksum 可以用于 provenance，不要塞进 gameplay hash。这样未来更新官方树数据时，测试能告诉你“数据源变了”，而不是神秘地说 fixture 不存在。
五是 CDN/美术依赖和确定性构建是两回事。 运行时图标如果还依赖实时网络可用性，就会让无头视觉门禁受外部服务状态影响。美术来源可以是 poedb CDN，但工程验证应尽量消费已经固定在项目内、带来源记录的资产。这个治理不需要采购，也不涉及玩法授权。
六是绘制 LOD 不得承担 hit-test 真相。 如果为了性能把“没画的节点”直接从交互集合里删掉，WO-05 就不是表现优化，而是在偷偷改被动域。绘制集、资源驻留集、可交互节点身份必须分开。
七是测试总数本身不能成为门禁常量。 当前 
394 / 394、
14 / 14 是基线证据，不是未来验收式。后续门槛应写“发现的全部测试 0 fail”，否则新增契约反而会因为分母变化把自动脚本弄红。
八是 
UNRESOLVED_WITH_SOURCE 不能等同于 blocker。 对那 70 个未获导演授权的新轴，它应该进入报告并使对应节点保持 unsupported；它不能把 S6P 整体卡死，更不能成为代理自动实现新 StatId 的借口。

授权边界
这批无人值守执行中可以直接授权： WO-02、WO-04A、WO-03、WO-04B、WO-05；以及它们需要的运行时代码修正、契约测试、只读审计/报告生成器、三进程验证脚本、现有资产引用/缓存/LOD 治理、review 证据文档。新增的是验证能力和已有语义正确性，不是玩法域。
必须导演另行授权、因此不进入上述任何执行队列： 新 Stat 轴及相应运行时语义、药剂真正玩法、珠宝/时光珠宝、升华、新技能、冰冷/闪电/ES/格挡/压制/召唤/图腾/战吼/异常/充能/吸取/DoT 等缺失 gameplay domains，以及购买资产、ORK/TopDown Engine 或其他框架切换。
最后给这轮 S6P 一个非常具体的结束定义：不是“2429 节点覆盖率提高到了多少”，而是“允许消费点数的节点/选项，100% 能由现有游戏系统兑现；无法兑现的，100% 在消费点数以前被拒绝；所有已允许的 build truth 都能被确定性地识别、快照和重放；表现层改变永远不能改变这份真相。”
这比追 531 → 1000 → 2000 的数字有价值，也更符合你这套工程现在真正缺的骨架。
来源
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。