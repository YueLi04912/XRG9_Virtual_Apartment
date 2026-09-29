

## Codely Structured Memories

### User
- [2026-09-26 20:03:24] 用户在团队中的分工：只负责场景搭建（scene building），不涉及交互脚本/标注系统等队友负责的部分。

### Feedback

### Project
- [2026-09-26 19:54:02] COMP5424 课程项目（Group 9，客户 Cedar & Stone Architects）：VR 虚拟公寓设计评审。Phase 1 文档在 C:\Users\weiyi\Desktop\XR group9\Phase 1_ Requirements and Prototype Proposal.docx。核心场景是单一厨房空间（Kitchen Clearance Review），采用 split-fidelity 灰盒路线：固定结构=统一灰色、临时元素=彩色（REQ-N01）；所有元素严格 1:1 真实尺寸（REQ-N04）；明确不要自定义贴图/复杂 shader/光照烘焙；交互仅限冰箱/烤箱门等关键家电（REQ-F04）；2.0×2.0m 活动范围；无线 PC-VR + SteamVR 投屏。Must-have 需求：F01 1:1行走、F02 标注+语音备忘录、F03 传送+SnapTurn、N01 颜色编码、F05 VR内引导板、N03 CSV/音频本地导出、N04 1:1尺寸。Should-have：F04 开门交互、F06 穿墙震动。
- [2026-09-27 23:07:59] KayKit Restaurant Bits 厨房8件已入 Assets/Prefabs（2026-09-27）：均为 FBX 模型变体，根节点带 1:1 缩放覆盖（FBX 本地轴 X=宽/Y=深/Z=高，配 -90°X 旋转，地面在本地 Z=0）；材质经各 FBX .meta 的 externalObjects（材质名 "restaurant"）重映射到 Fixed.mat（冰箱/烤箱/两地柜/灶台/整墙 6 件）与 Non-fixed.mat（锅、圆桌 2 件）。注意：wall_decorated 进深被压缩到 0.65m 仅作示意、kitchencounter_straight_a 原生矮柜被拉高到 0.9m、灶台面板 pivot 离地约 0.1m；正式 1:1 摆台建议用地柜+水槽柜分体件。

### Reference

