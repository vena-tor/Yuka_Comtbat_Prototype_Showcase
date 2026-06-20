# combat_prototype 개발 인수인계서

> 이 문서의 0번과 10번 항목을 최신 상태의 기준으로 본다. 8번 디버깅 이력의 과거 표현은 당시 문제를 기록한 역사다.

## 0. 최신 갱신 요약 (2026-06-13 캡슐 프로토타입 코드 스코프 종료)

개발 시작일은 2026-05-26, 현재 완료일은 2026-06-13이다.
경과 18일, 시작일과 완료일을 모두 포함하면 19일 만에 캡슐 전투 프로토타입의 코드 기능 범위를 닫았다.

이번 갱신에서 2026-06-12 이후 추가된 핵심 기능은 다음 3개다.

### 완료 1: Enemy 피격 리액션 / 사망 연출 강화

* Enemy가 피격되면 기존 HitStun, 공격 중단, 넉백이 함께 작동한다.
* HP가 0이 되면 EnemySimpleAI / EnemyAttack / EnemyFleeAI를 정지한다.
* Rigidbody 이동을 멈추고 물리 간섭을 차단한 뒤, 캡슐이 천천히 옆으로 기울어 눕는 사망 연출을 실행한다.
* 즉시 증발하지 않고 짧게 바닥에 남아 사망이 눈에 읽히게 만들었다.
* 사망 연출 종료 후 삭제하지 않고 `CorpseObject` 상태로 전환한다.
* 캡슐 단계에서는 마인크래프트식으로 단순하지만, 피격·사망 결과가 명확하게 전달되는 것을 우선한다.

### 완료 2: Enemy 제한적 백스텝 / Guard

* PlayerController에 `AttackStartSequence`를 추가했다.
  → 1타 / 2타 / 3타가 시작될 때마다 번호가 증가한다.
  → EnemySimpleAI가 단순 `IsAttacking` bool이 아니라 각 공격 시작을 별도 사건으로 감지한다.
* EnemySimpleAI에 `Backstep` 상태를 추가했다.
  → 낮은 확률, 짧은 거리, 별도 쿨다운으로만 발동
  → Player를 바라본 채 뒤로 물러남
  → 회피 무적 없음
  → 실제 검 판정을 벗어나지 못하면 맞으면서 물러날 수 있음
* EnemySimpleAI에 `Guard` 상태를 추가했다.
  → 일반 공격을 완전히 무효화하지 않고 `guardDamageMultiplier`만큼 피해 감소
  → 넉백과 HitStun은 그대로 적용
  → 한 번 막으면 Guard 해제
* EnemyHealth / EnemyHitBox에 `ignoreGuard` 오버로드를 추가했다.
* BreakCharge는 `ignoreGuard = true`로 처리한다.
  → 돌파는 Guard 피해 감소를 무시
  → “돌파에 방어가 깨짐” 로그 출력
  → 일반 공격 20이 Guard에서 10으로 감소하고, 돌파 40은 그대로 적용되는 것 확인 완료
* 방향은 “Enemy가 잘 막고 잘 피해서 오래 사는 AI”가 아니라, 유카 앞에서 수를 써보지만 결국 파훼되는 행동이다.

### 완료 3: 시신 방패 축소판

* `CorpseObject.cs`, `CorpseShield.cs`를 추가했다.
* Enemy 사망 연출 종료 후 시신을 삭제하지 않고 `CorpseObject`로 전환한다.
* Player는 가까운 시신을 E 입력으로 집어 전방 HoldPoint에 부착할 수 있다.
* 시신을 든 상태에서도 이동 가능하며, 평상시 걷기 속도는 유지한다.
* Shift 달리기도 가능하되 평상시 달리기보다 느린 `corpseShieldRunSpeed`를 사용한다.
* 시신 방패 유지 중 방어적 입력은 제한한다.
  → Space 구르기 불가
  → 우클릭 일반 막기 불가
  → Left Shift + Right Mouse 돌파 불가
* 좌클릭 공격은 허용한다.
  → 공격 입력 순간 시신 방패 권한을 상실하고 시신을 소모
  → 같은 입력으로 1타 공격 시작
* Enemy 공격을 받으면 시신이 공격 1회를 대신 받고 파괴된다.
  → Player HP 감소 없음
  → `PlayerDamageResult.Blocked`로 반환
* 시신 방패는 무료 방어 장비가 아니라, “한 번 받아낼 것인가 / 버리고 먼저 공격할 것인가”를 고르는 일회성 전투 자원이다.

### 코드 스코프 종료 판단

현재 캡슐 프로토타입에는 다음 유카 고유 기능이 들어갔다.

* 돌파
  → 살아 있는 Enemy의 공격과 자세를 정면에서 짓누름
* 시신 방패
  → 죽은 Enemy까지 즉석 전투 자원으로 사용

마법 해제 축소판은 이번 캡슐 프로토타입에서 제외한다.

* 해제 대상이 되는 적 마법 / 마법 구조체 / 투사체 시스템부터 새로 만들어야 한다.
* 지금 넣으면 “마법 공 하나 생성 후 삭제” 수준의 고립된 시험 기능이 될 가능성이 크다.
* 적 마법사와 실제 마법 전투가 들어가는 취업 포트폴리오 단계 또는 완전판 Vertical Slice에서 구현한다.

현재 판단:

1. 락온 이동 2차 구현 ✅
2. 돌파 디테일 강화 ✅
3. 부위별 EnemyHitBox 구조 ✅ / 실제 배치는 에셋 이후
4. 다수 Enemy 공격권 조율 ✅
5. Enemy 피격 / 사망 연출 ✅
6. Enemy 제한적 백스텝 / Guard ✅
7. 시신 방패 축소판 ✅
8. 마법 해제 축소판 ⏸ 에셋·적 마법 시스템 이후 보류

이 시점부터는 새 코드 기능을 계속 늘리지 않는다.
다음 단계는 캡슐 프로토타입 보존 브랜치를 만든 뒤, 메인 개발에서 에셋·애니메이션·연출·UI를 적용해 기본 취업 포트폴리오 데모로 발전시키는 것이다.

현재 작업 커밋 후보:

* `Add enemy defense reactions and corpse shield system`
* 전체 마감 커밋: `Complete capsule combat prototype`


---

## 1. 프로젝트 개요

Unity 3D 액션 전투 프로토타입.
목표는 유카 전투 시스템의 최소 조작감을 검증하는 것.

현재 단계는 완성 게임이 아니라 전투 조작 실험장이다.
그래픽/애니메이션보다 먼저 이동, 카메라, 구르기, 막기, 공격, 피격 판정, 적 반응, 전투 피드백을 만든다.

현재 개발 방향은 다음과 같다.

* 기본 액션 조작감 구축
* 공격/피격/회피 판정의 기초 구현
* Enemy와의 상호작용 구현
* 유카 고유 전투 스킬의 프로토타입화
* 데모용 전투 시스템의 최소 재미 확보
* Enemy의 물리 몸통, 피격 판정, 락온 기준점, AI 행동을 점진적으로 분리
* 락온 이동, 돌파 충격감, 다수 Enemy 공격권 조율을 통해 전투 자세와 다수전 리듬을 강화
* Enemy 백스텝/Guard, 피격·사망 연출로 맞아주는 상대의 행동 밀도를 강화
* 돌파와 시신 방패를 통해 유카 고유 전투 정체성을 캡슐 단계에서 검증
* 캡슐 프로토타입 코드 스코프를 종료하고 에셋·애니메이션 단계로 전환

---

## 2. 개발 방식

사용자는 코딩 기초부터 차근차근 배우는 방식이 맞지 않는다.
피라미드, 수강신청, 성적관리 같은 일반 예제가 아니라 게임 기능을 바로 구현한다.

설명은 기능 중심으로 한다.

예:

* "구르기 중 공격 불가"
* "공격 중 다음 입력을 예약"
* "카메라 기준 이동"
* "Enemy 레이어만 공격 판정"
* "검 기반 히트박스로 적중 판정"
* "공격 판정은 선딜/활성/후딜 구조로 나눈다"
* "Enemy가 도망가고, 돌파로 들이받는다"
* "Left Shift + Right Mouse를 유지하는 동안 돌파 상태를 유지한다"
* "Enemy 본체와 EnemyHitBox를 분리한다"
* "락온은 대상 선택, 인디케이터 표시, 소프트 카메라 제한까지 구현한다"
* "Enemy AI는 공격 전 단계의 행동 골격부터 만든다"

문법 설명은 필요한 순간에만 짧게 한다.

현재 개발 원칙:

* 기존 구조를 함부로 대규모 리팩터링하지 않는다.
* 작은 기능 단위로 추가한다.
* 기능 하나가 성공하면 테스트하고 커밋한다.
* 작동하는 상태를 GitHub에 자주 저장한다.
* 재미가 너무 떨어지면 유카 고유 스킬이나 시각적 피드백 작업을 섞는다.
* 유카 고유 스킬은 가능하면 PlayerController에 전부 넣지 말고 별도 스크립트로 분리한다.
* 기존 스크립트가 작동 중이면 삭제하지 말고 백업용으로 유지한다.
* 새 기능은 기존 기능과 충돌하지 않게 컴포넌트 단위로 붙이고 뺄 수 있게 만든다.

---

## 3. 현재 구현된 기능 수정/추가

### 기본 환경

* Player 캡슐 생성
* Ground 생성
* Enemy 캡슐 생성
* GitHub 저장소 연결 완료
* MCP 서버 구동 및 Codex 활용 가능 상태 확인

### Player 조작

* WASD 이동
* Left Shift 달리기
* 마우스 궤도 카메라
* 카메라 기준 이동
* 이동 방향으로 Player 회전
* Space 구르기
* 우클릭 막기
* 좌클릭 공격
* 좌클릭 연타 1타 → 2타 → 3타 콤보

### 구르기 무적

* 구르기 무적 시간 추가
  → Dodge 상태 중 `dodgeInvincibleStartTime` ~ `dodgeInvincibleEndTime` 구간에서 `IsInvincible = true`
  → Console 로그로 무적 시작/종료 확인 가능
  → EnemyAttack이 PlayerHealth.TakeDamage를 호출했을 때, PlayerController.IsInvincible이 true면 데미지 무시
  → 현재 Enemy 공격과 실제로 연결되어 있으며, 구르기 타이밍이 맞으면 “구르기 무적 중 - 데미지 무시” 로그 출력

### Player HP / 피격 / 막기 / 피드백

* PlayerHealth.cs 추가 및 갱신
  → Player의 maxHp / currentHp 관리
  → CurrentHp / MaxHp / IsDead 프로퍼티 제공
  → EnemyAttack에서 PlayerHealth.TakeDamage(damage, attackerPosition)를 호출하면 HP 감소
  → HP가 0이 되어도 테스트 편의상 Player 오브젝트를 Destroy하지 않고 “Player 사망” 로그만 출력
  → Heal / FullHeal 함수 포함
  → 피격 후 짧은 damageCooldown으로 연속 다단히트가 한 번에 우수수 들어오는 것을 방지
  → 데미지 상한 같은 하드 제한은 넣지 않음
  → 적 공격을 즉사급으로 만들지 않는 것은 PlayerHealth 기능이 아니라 앞으로 지킬 전투 기획 규칙

* PlayerDamageResult enum 추가
  → PlayerHealth.TakeDamage가 bool이 아니라 PlayerDamageResult를 반환
  → Damaged: 실제 HP 감소
  → Dodged: 구르기 무적으로 데미지 무시
  → Blocked: 막기로 데미지 무시
  → Cooldown: 피격 쿨다운으로 데미지 무시
  → Invalid: 사망/잘못된 데미지 등 무효 처리
  → EnemyAttack 로그가 막기/회피/실제 피격을 구분할 수 있게 됨

* 막기 데미지 무시 및 피드백 연결
  → PlayerController에 IsBlocking 공개 프로퍼티 추가
  → Right Mouse 유지 중 PlayerState.Block이면 PlayerHealth가 막기 상태로 판단
  → 막기 중 Enemy 공격을 맞으면 “막기 성공 - 데미지 무시” 로그 출력
  → EnemyAttack 쪽에서는 “공격이 막힘”으로 분리 출력
  → 막기 성공 시 CameraFollow.Shake(blockShakeDuration, blockShakeStrength) 호출
  → 현재 막기 성공 시 데미지 0
  → 아직 막기 이펙트, 가드 충격, 스태미나, 가드 브레이크는 없음

* Player 피격 피드백 강화
  → 실제 HP가 감소하는 경우에만 피격 피드백 실행
  → Player 피격 시 CameraFollow.Shake(hitShakeDuration, hitShakeStrength) 호출
  → attackerPosition 기준으로 Player를 반대 방향으로 넉백
  → 짧은 hitStunDuration 동안 PlayerController.SetExternalActionLock(true)로 조작 잠금
  → 넉백이 PlayerController 이동 입력에 바로 덮어씌워지지 않도록 처리
  → 막기/구르기/쿨다운으로 데미지를 무시한 경우에는 피격 넉백이 발생하지 않음

### 공격 판정

* 검 기반 공격 히트박스
  → 기존 AttackPoint + Sphere 방식에서 `weaponHitBoxCenter + OverlapBox` 방식으로 전환
  → 검 에셋 위치를 따라가는 박스형 히트박스 사용
  → 실제 검날과 완전히 일치시키기보다 조작감을 위해 약간 두꺼운 “몽둥이형” 판정으로 조정

* 공격 판정 타이밍 조절
  → 공격 시작 즉시 적중하지 않고, comboStep별 hitStartTime ~ hitEndTime 구간에서만 판정 활성
  → 선딜 / 활성 / 후딜 구조의 기초 구현
  → 추후 애니메이션과 연결할 예정

* 공격 범위 시각화
  → 히트박스 위치와 크기를 Scene/Game 테스트에서 확인 가능
  → 노란색은 비활성/확인용
  → 빨간색은 실제 판정 활성 구간 표시

* EnemyHitBox 우선 감지
  → Player 공격은 EnemyHitBox를 우선 감지하고, 없으면 EnemyHealth를 fallback으로 찾는 구조
  → 기존 Enemy 구조와 새 EnemyHitBox 구조가 모두 작동하도록 안전장치 유지

### 돌파

* BreakCharge.cs 추가
  → 유카 고유 스킬 “돌파”의 축소판 구현
  → `Left Shift + Right Mouse` 조합으로 발동
  → 입력 순서와 상관없이 두 입력이 함께 유지되면 돌파 시작
  → 두 입력을 계속 누르고 있는 동안 돌파 상태 유지
  → 둘 중 하나라도 떼면 돌파 리커버리 후 조작 복귀
  → 돌파 중에는 PlayerController를 외부 잠금 처리하여 이동/공격/구르기/막기 입력이 충돌하지 않게 함
  → 돌파 중 WASD 입력이 있으면 카메라 기준 입력 방향으로 진행 방향이 갱신됨
  → WASD 입력이 없을 때만 카메라 정면 방향으로 돌파함
  → Enemy와 충돌하면 데미지와 강한 넉백 발생
  → 돌파 상태 갱신은 BreakCharge.FixedUpdate에서 처리함
  → 돌파 충돌 시 카메라 흔들림 추가
  → 돌파 충돌 시 임시 충격 이펙트 생성
  → EnemyHitBox를 우선 감지하고, 없으면 EnemyHealth를 fallback으로 찾음

* BreakCharge.cs 돌파 방향 수정
  → 기존 돌파는 WASD 입력과 관계없이 cameraTransform.forward 방향으로 진행되는 문제가 있었음
  → 락온 중 S + Left Shift + Right Mouse를 입력해도 Enemy 방향으로 들이박는 문제가 발생
  → 수정 후 WASD 입력이 있으면 카메라 기준 입력 방향으로 돌파
  → W + 돌파: 카메라 기준 전방 돌파
  → S + 돌파: 카메라 기준 후방 돌파
  → A/D + 돌파: 카메라 기준 좌우 돌파
  → WASD 입력 없이 돌파하면 기존처럼 카메라 정면으로 돌파
  → 락온 중에도 후방 이탈 돌파, 측면 기동 돌파가 가능해짐

### Enemy 관련

* EnemyHealth 체력 시스템

* Enemy 사망 처리

* EnemyFleeAI.cs 추가
  → Player가 일정 거리 안에 들어오면 Enemy가 반대 방향으로 도망감
  → stopDistance 이상 멀어지면 정지함
  → 현재는 백업용/비교용 AI로 유지
  → EnemySimpleAI를 사용할 때는 EnemyFleeAI 컴포넌트를 비활성화한다

* Enemy 피격 넉백 추가
  → `EnemyHealth.TakeDamage(damage, attackerPosition)` 형태로 공격자 위치를 받아 넉백 방향 계산
  → 돌파용 강한 넉백을 위해 knockbackMultiplier 오버로드 추가
  → 피격 직후 EnemyFleeAI를 잠깐 정지시켜 넉백이 바로 덮어씌워지지 않게 처리
  → 추후 EnemySimpleAI에도 피격 정지/HitStun을 연결할 예정

* EnemyHitBox.cs 추가
  → Enemy 본체 오브젝트와 실제 피격 판정을 분리
  → Enemy 자식 오브젝트에 EnemyHitBox를 두고, 해당 Collider를 피격 판정으로 사용
  → Enemy 본체 Capsule Collider는 물리 충돌/이동 몸통 역할
  → EnemyHitBox 자식 Collider는 공격/돌파가 감지하는 피격 판정 역할
  → EnemyHitBox Collider는 Is Trigger를 체크
  → EnemyHitBox 오브젝트 Layer는 Enemy로 설정
  → 공격과 돌파는 EnemyHitBox를 우선 감지하고, 최종 데미지는 부모 EnemyHealth로 전달
  → 추후 머리/몸통/팔/다리 등 부위별 HitBox 확장 가능
  → 추후 락온 중심점, 약점 표시, 무장해제 판정과 연결 가능

* EnemySimpleAI.cs 추가
  → 기존 Patrol / AlertStare / AlertCircle / CloseCircle / Approach / Flee / Return 중심 구조를 더 세분화
  → 원거리 발견/응시 상태와 근거리 대치 상태를 분리
  → 현재 상태 구조는 Patrol / AlertStare / AlertCircle / Approach / CloseFaceOff / CloseCircle / Flee / Return
  → AlertStare는 원거리에서 Player를 발견하고 응시하는 상태
  → AlertCircle은 원거리에서 Player를 바라보며 시계/반시계 방향으로 선회하는 상태
  → Approach는 Player가 AlertStare 구간보다 가까이 들어왔을 때 추격/압박하는 상태
  → CloseFaceOff는 근거리에서 Player를 바라보며 대치하는 상태
  → CloseCircle은 근거리에서 Player를 바라보며 선회하는 상태
  → Flee는 너무 가까이 붙었을 때 후퇴하는 상태
  → Return은 Player를 놓쳤을 때 시작 위치로 돌아가는 상태
  → 현재 EnemyAttack과 연결되어 CloseFaceOff / CloseCircle 상태에서 공격 시도 가능
  → 아직 Enemy 방어, 회피, 복수 공격 패턴은 구현하지 않음

* EnemySimpleAI 거리 구조
  → Detect Distance 안으로 Player가 들어오면 Enemy가 Player를 발견
  → Alert Min Distance ~ Alert Max Distance 사이에서는 원거리 AlertStare / AlertCircle 유지
  → Alert Min Distance 안으로 Player가 들어오면 Approach 상태로 전환
  → Close Face Off Distance 안으로 들어오면 CloseFaceOff 상태로 전환
  → Flee Distance 안으로 너무 가까워지면 Flee 상태로 전환
  → Lose Distance 밖으로 멀어지면 Return 상태로 전환
  → Player가 짧은 거리에서 와리가리해도 Enemy가 즉시 추격하지 않고 상태를 유지하도록 조정

* EnemyAttack.cs 추가
  → Enemy의 1타 공격 처리 전용 스크립트
  → 공격 가능 거리, 공격 쿨다운, 선딜, 판정 활성 시간, 후딜 관리
  → 공격 판정은 Physics.OverlapBox 기반 박스 판정
  → Player Layer에 속한 Player Collider를 감지하면 PlayerHealth.TakeDamage 호출
  → EnemySimpleAI는 공격 판단만 하고, 실제 공격 실행은 EnemyAttack이 담당
  → EnemySimpleAI의 CloseFaceOff / CloseCircle 상태에서 EnemyAttack.CanAttack(target)이 true면 StartAttack(target) 호출
  → 공격 중에는 EnemySimpleAI가 StopMoving 후 상태 전환을 잠시 멈춤
  → 현재 공격은 단일 1타 공격이며, 애니메이션/이펙트/사운드는 아직 없음
  → 기본 데미지는 약한 값으로 운용한다. 현재 테스트 기준 Attack Damage 10

* EnemyAttackCoordinator.cs 추가
  → 다수 Enemy 공격권 조율 전용 중앙 관리자
  → Scene Hierarchy에 빈 오브젝트 `EnemyAttackCoordinator`를 만들고 스크립트를 붙임
  → `maxSimultaneousAttackers = 1` 기준 한 번에 한 Enemy만 공격 가능
  → EnemyAttack은 공격 전에 슬롯 확인/획득
  → 공격 종료, 피격 중단, 비활성화 시 공격권 반납
  → 두 마리 이상 테스트에서 한 놈이 공격하는 동안 다른 Enemy는 대치/선회하는 흐름 확인

### Enemy 피격 / 사망 연출 강화

* Enemy 피격 시 넉백, HitStun, EnemyAttack.InterruptAttack이 함께 작동한다.
* HP 0 시 Enemy AI와 공격 컴포넌트를 비활성화한다.
* Rigidbody 수평 이동과 회전을 정지하고 `isKinematic = true`로 전환한다.
* 캡슐이 `deathFallDuration` 동안 `deathFallAngle`만큼 천천히 기울어진다.
* Pivot이 중앙이라 바닥에 박히는 문제를 줄이기 위해 `deathLowerDistance`만큼 위치도 함께 내린다.
* `deathRemainDuration` 동안 누운 상태를 유지한 뒤 `CorpseObject`로 전환한다.
* 사망한 Enemy는 더 이상 공격·락온·피격 대상이 아니며, 시신 방패용 자원으로만 남는다.

### Enemy 제한적 백스텝 / Guard

* PlayerController의 `AttackStartSequence`로 각 콤보 타격 시작을 별도 감지한다.
* `Backstep`
  → 낮은 확률과 쿨다운
  → Player 반대 방향 이동
  → 몸은 Player를 계속 바라봄
  → 무적 없음
  → 늦으면 맞으면서 물러날 수 있음
* `Guard`
  → 낮은 확률과 쿨다운
  → 일반 공격 피해 일부 감소
  → 넉백과 경직 유지
  → 피격 후 Guard 해제
* Guard 중 일반 공격은 `GuardDamageMultiplier`를 적용한다.
* 돌파는 `ignoreGuard = true`로 Guard를 깨고 원래 데미지를 적용한다.
* 테스트용 `startInGuardForTest`, `holdGuardForTest` 옵션으로 돌파 가드 브레이크를 즉시 재현할 수 있다.

### 시신 방패

* `CorpseObject.cs`
  → 사망한 Enemy가 시신으로 사용 가능한지 관리
  → 집었을 때 Rigidbody / Collider 간섭 제거
  → Player HoldPoint 자식으로 부착
  → 방어 소모 또는 공격 전환 시 파괴
* `CorpseShield.cs`
  → 가까운 시신 탐색
  → E 입력으로 집기
  → Player 전방 HoldPoint에 시신 부착
  → 공격 1회 대신 받기
  → 공격 전환 시 시신 소모
* 시신 방패 상태 규칙:
  → 이동 가능
  → 평상시 걷기 속도 유지
  → Shift 전용 달리기 가능, 평상시 달리기보다 느림
  → 구르기 불가
  → 우클릭 막기 불가
  → 돌파 불가
  → 좌클릭 공격 가능, 단 공격 순간 시신 방패 상실
* PlayerHealth는 구르기 판정 다음, 일반 막기 판정 전에 `CorpseShield.TryBlockAttack()`을 확인한다.
* 시신이 공격을 대신 받으면 `PlayerDamageResult.Blocked`를 반환해 EnemyAttack 쪽 로그도 “공격이 막힘”으로 처리한다.

### 락온 관련

* LockOnSystem.cs 추가
  → Tab 또는 Middle Mouse 입력으로 근처 Enemy를 락온
  → 다시 입력하면 락온 해제
  → 락온된 Enemy에 흰색 인디케이터 표시
  → Enemy가 사망하면 락온 인디케이터도 제거
  → 락온 인디케이터는 Enemy 몸 중앙 쪽에 보이도록 LockOnTargetPoint를 기준으로 배치
  → LockOnTargetPoint는 Enemy 자식 오브젝트로 생성
  → 인디케이터는 target point에서 카메라 쪽으로 살짝 빼서 몸에 박힌 느낌을 유지하면서도 보이게 함
  → 현재 락온은 “대상 선택 + 인디케이터 표시 + 소프트 카메라 제한”까지 구현된 상태
  → 카메라를 Enemy 중앙에 강제 고정하지 않고, Enemy가 화면 밖으로 완전히 벗어나지 않게 제한한다.

* 락온 카메라 보정 실험
  → 카메라가 Enemy를 강제로 바라보게 하면 카메라 기준 이동과 충돌함
  → W 입력 시 Enemy 쪽으로 빨려 들어가는 느낌이 발생
  → 돌파도 cameraTransform.forward를 따라가기 때문에 Enemy에게 우다다다 직진하는 느낌이 강해짐
  → 현재는 소프트 락온 방식으로 다시 활성화.
  → 추후 LockOnMovement / LockOnCamera를 별도 설계할 예정

* LockOnSystem.cs / CameraFollow.cs 수정
  → 기존 락온은 대상 선택과 인디케이터 표시만 가능했음
  → 이번 수정으로 락온 중 카메라가 Enemy를 화면 밖으로 완전히 놓치지 않도록 소프트 락온 방식 추가
  → 마우스로 카메라를 돌릴 수는 있지만, 락온 대상 기준으로 yaw/pitch가 일정 각도 이상 벗어나지 않도록 제한
  → 하드락처럼 Enemy를 화면 중앙에 강제로 고정하지 않음
  → 카메라 기준 이동과 충돌하던 기존 문제를 줄이기 위해 소프트 제한 방식 사용
  → Lock On Max Yaw Offset / Lock On Max Pitch Offset 값으로 카메라 구속 강도 조절 가능

* 락온 거리 조정
  → Enemy가 아직 Player를 인식하지 않아도 Player는 멀리서 Enemy를 락온할 수 있게 조정
  → EnemySimpleAI의 detectDistance와 LockOnSystem의 lockRange는 서로 다른 값으로 관리
  → Enemy detectDistance는 Enemy가 Player를 발견하는 거리
  → LockOnSystem lockRange는 Player가 Enemy를 락온할 수 있는 거리
  → 현재 의도는 “Enemy가 아직 반응하지 않아도 Player는 먼저 락온 가능”이다

### HP UI / HUD

* PlayerHUD.cs 추가
  → HUDCanvas에 붙이는 Player HP UI 갱신 스크립트
  → PlayerHealth.CurrentHp / MaxHp를 읽어 Slider 값을 갱신
  → Player HP Bar는 화면 왼쪽 아래 배치
  → Player HP Fill 색상은 초록색 계열로 설정
  → 맞으면 줄고, 막기/구르기 성공 시 줄지 않는 것 확인 완료

* EnemyHealthBar.cs 추가
  → Enemy 머리 위 World Space Canvas에 붙이는 Enemy HP UI 갱신 스크립트
  → EnemyHealth.CurrentHp / MaxHp를 읽어 Slider 값을 갱신
  → Enemy 위치 + worldOffset을 따라다님
  → 카메라 방향을 바라보도록 회전 처리
  → Enemy HP Bar는 적 머리 위 배치
  → Enemy HP Fill 색상은 붉은색 계열로 설정
  → Enemy가 사망하면 HP Bar Canvas를 숨기고, 시신 방패용 CorpseObject로 전환

* UI 배치 기준
  → Player HP: 화면 왼쪽 아래 HUD
  → 일반 Enemy HP: 적 머리 위 World Space HP Bar
  → 보스/락온 대상 HP는 추후 화면 하단 중앙 또는 별도 HUD로 검토

---

### 에셋 관련

* 검 에셋 삽입
  → 최초 삽입 시 머티리얼/셰이더 문제로 핑크색 표시 발생
  → 머티리얼 수정으로 정상 표시 해결
  → Player 자식으로 배치하여 이동/회전 추적

---

## 4. 현재 주요 오브젝트 구조

Hierarchy 구조:

Main Camera

* CameraFollow
  → 마우스 궤도 카메라
  → 돌파 명중 카메라 흔들림
  → 막기/피격 카메라 흔들림
  → 락온 중 소프트 카메라 제한 처리

HUDCanvas

* PlayerHUD
* PlayerHPBar
  → Screen Space UI
  → Player HP 표시
  → 왼쪽 아래 배치
  → Fill 색상은 초록색 계열

EnemyAttackCoordinator

* EnemyAttackCoordinator.cs
  → 다수 Enemy 공격권 조율
  → Max Simultaneous Attackers = 1

Ground

* Mesh Collider
* Rigidbody 없음

Player

* Capsule Collider
* Rigidbody
* PlayerController
* PlayerHealth
* BreakCharge
* CorpseShield
* CorpseShieldHoldPoint
  → Play 시작 시 자동 생성 가능
  → 들고 있는 시신을 Player 전방에 부착
* LockOnSystem
* Nose
* AttackPoint 또는 기존 판정 기준 오브젝트
* Sword

  * SwordHitBox 또는 weaponHitBoxCenter

Enemy

* Capsule Collider
* Rigidbody
* EnemyHealth
  * Enemy 피격 / Guard / 넉백 / 사망 연출 / Corpse 전환
* EnemyFleeAI
  * 현재 EnemySimpleAI 사용 시 비활성화 권장
* EnemySimpleAI
  * Patrol / AlertStare / AlertCircle / Approach / ApproachAttack / CloseFaceOff / CloseCircle / Backstep / Guard / Flee / Return
* EnemyAttack
  * 1타 / 2타 연격 / 공격 후 행동 분기 / 피격 중단 / 공격권 조율
* EnemyHitBox
  * Capsule Collider
  * EnemyHitBox.cs
* LockOnTargetPoint
* CorpseObject
  → 사망 연출 종료 후 추가 또는 활성화
  → 시신 방패 탐색 대상
* EnemyHPBarCanvas
  * World Space Canvas
  * EnemyHealthBar
  * EnemyHPBar Slider
  → Enemy 머리 위 HP 표시
  → Fill 색상은 붉은색 계열

주의:

Ground에는 Rigidbody를 붙이면 안 된다.
Ground에 Rigidbody가 붙으면 카메라/물리 떨림이 발생했다.

Enemy 본체 Capsule Collider는 물리 충돌/이동 몸통 역할을 한다.
EnemyHitBox 자식 오브젝트의 Capsule Collider는 피격 판정 역할을 한다.
EnemyHitBox의 Collider는 Is Trigger를 체크한다.
EnemyHitBox 오브젝트의 Layer는 Enemy로 설정한다.

LockOnTargetPoint는 락온 인디케이터가 붙을 기준점이다.
Enemy의 복부/가슴 언저리에 배치한다.
추천 Local Position은 대략 `(0, 1.0~1.2, 0)`이다.

현재 공격 판정은 기존 AttackPoint 구형 판정이 아니라, 검 자식 오브젝트에 둔 `weaponHitBoxCenter` 기준의 Box 판정으로 전환 중이다.

AttackPoint는 완전히 삭제하지 말고, 필요하면 백업용으로 남겨둔다.
새 히트박스 구조가 안정화된 뒤 제거해도 된다.

BreakCharge는 PlayerController와 같은 Player 오브젝트에 붙인다.
돌파 중 PlayerController와 입력/이동 처리가 충돌하지 않도록 PlayerController의 외부 액션 잠금 기능을 사용한다.

LockOnSystem도 Player 오브젝트에 붙인다.
락온 인디케이터는 LockOnSystem이 생성/삭제한다.

LockOnSystem은 Player 오브젝트에 붙인다.
CameraFollow는 Main Camera에 붙인다.
LockOnSystem에서 CameraFollow를 참조해야 소프트 락온 카메라 제한이 작동한다.

EnemySimpleAI를 사용할 때는 EnemyFleeAI 컴포넌트를 비활성화한다.
두 AI가 동시에 Rigidbody linearVelocity를 건드리면 이동이 꼬일 수 있다.

Player 루트 오브젝트의 Layer는 Player로 설정한다.
EnemyAttack의 Player Layer도 Player로 체크해야 한다.
EnemyAttack의 Player Layer는 Hierarchy 오브젝트를 넣는 칸이 아니라 LayerMask 선택 칸이다.
Player 오브젝트가 Default 레이어로 남아 있으면 Enemy 공격 판정이 Player를 감지하지 못한다.

---

## 5. 코드 규칙

Unity Rigidbody 속도 설정은 반드시 `linearVelocity` 기준으로 작성한다.

사용:

```csharp
rb.linearVelocity = new Vector3(x, rb.linearVelocity.y, z);
```

피할 것:

```csharp
rb.velocity = ...
```

Player 회전은 Rigidbody가 있는 캐릭터이므로 `transform.rotation` 직접 변경보다 `rb.MoveRotation`을 우선한다.

Enemy와 Player가 충돌할 때 물리 회전이 생길 수 있으므로, 필요한 경우 `FixedUpdate`에서 angularVelocity를 제거한다.

```csharp
rb.angularVelocity = Vector3.zero;
```

입력은 현재 Old Input Manager 또는 Both 설정 기준이다.
WASD 이동은 `Input.GetKey` 방식으로 직접 읽는다.

`Input.GetAxisRaw("Horizontal")` / `Input.GetAxisRaw("Vertical")` 방식은 패드/가상 입력축 문제로 주인공이 혼자 도는 문제가 있었으므로 피한다.

Unity 콘솔에 `Input Manager is marked for deprecation` 경고가 뜰 수 있다.
현재는 에러가 아니라 경고이며, 프로토타입 단계에서는 Old Input Manager 기반 입력을 유지한다.
패드 지원, 키 리바인딩, UI 입력 분리 단계가 오면 New Input System 이전을 검토한다.

Assets 안에는 컴파일되지 않는 미완성 C# 코드 조각을 넣지 않는다.
Unity는 Assets 안의 `.cs` 파일을 전부 컴파일 대상으로 보기 때문에, 미완성 아이디어 코드는 `.md` 문서나 Assets 밖에 둔다.

유카 고유 스킬은 PlayerController에 계속 추가하지 말고, 가능하면 별도 스크립트로 분리한다.

예:

* BreakCharge.cs
* MagicDispel.cs
* WeaponDisarm.cs
* AutoParry.cs

Enemy AI도 가능하면 기존 단일 스크립트를 무리하게 확장하지 말고, 테스트 가능한 새 컴포넌트로 분리한다.

예:

* EnemyFleeAI.cs
* EnemySimpleAI.cs

---

## 6. 입력 방식

현재 조작:

* W/A/S/D: 이동
* Mouse: 카메라 회전
* Space: 구르기
* Right Mouse: 막기
* Left Mouse: 공격
* Left Mouse 연타: 콤보
* Left Shift: 달리기
* Left Shift + Right Mouse 유지: 돌파
* Tab 또는 Middle Mouse: 락온 / 락온 해제
* E: 가까운 시신 집기 / 시신 방패 유지·해제

현재 입력 구조:

* 이동 입력은 `Input.GetKey(KeyCode.W/A/S/D)` 방식
* 공격 입력은 `Input.GetMouseButtonDown(0)`
* 막기는 `Input.GetMouseButton(1)`
  → 우클릭 유지 중 PlayerState.Block
  → PlayerHealth에서 IsBlocking을 확인해 일반 Enemy 공격 데미지 무시
* 구르기는 `Input.GetKeyDown(KeyCode.Space)`
* 달리기는 `Input.GetKey(KeyCode.LeftShift)` 기준
* 돌파는 `Input.GetKey(LeftShift) && Input.GetMouseButton(1)` 기준
* 락온은 `Input.GetKeyDown(Tab)` 또는 `Input.GetMouseButtonDown(2)` 기준
* 시신 방패는 E 입력 기준
  → 시신을 든 동안 Space / Right Mouse / 돌파 입력 제한
  → Left Mouse 입력은 시신을 소모한 뒤 공격으로 전환
  → Shift 이동은 `corpseShieldRunSpeed` 사용

돌파 입력 규칙:

* Left Shift를 먼저 누른 뒤 Right Mouse를 눌러도 발동
* Right Mouse로 막기 중 Left Shift를 추가 입력해도 발동
* 두 입력을 거의 동시에 눌러도 발동
* 두 입력을 유지하는 동안 돌파 상태 지속
* 둘 중 하나라도 떼면 돌파 리커버리 진입
* 돌파 중 WASD 입력이 있으면 카메라 기준 WASD 방향으로 돌파한다.
* W + Left Shift + Right Mouse: 카메라 기준 전방 돌파
* S + Left Shift + Right Mouse: 카메라 기준 후방 돌파
* A/D + Left Shift + Right Mouse: 카메라 기준 좌우 돌파
* WASD 입력 없이 Left Shift + Right Mouse만 누르면 카메라 정면으로 돌파한다.
* 락온 중에도 돌파 방향은 WASD 입력을 우선한다.

락온 입력 규칙:

* Tab 또는 Middle Mouse로 가장 적절한 Enemy를 락온
* 락온 중 다시 입력하면 락온 해제
* 락온 대상이 사망하면 자동 해제
* 락온 중 카메라는 완전 자유 회전이 아니라, Enemy가 화면 밖으로 완전히 빠지지 않도록 소프트 제한을 받는다.
* 락온은 Enemy AI의 인식 여부와 별개로 Player가 먼저 걸 수 있다.

주의:

콤보는 공격 중 좌클릭 입력을 받아 다음 공격을 예약하는 구조다.
너무 빡빡한 입력 조건은 피하고, 프로토타입 단계에서는 비교적 관대하게 받는다.
돌파 입력은 PlayerController가 아니라 BreakCharge가 처리한다.
PlayerController는 BreakCharge가 `SetExternalActionLock(true)`를 호출했을 때 기본 조작을 멈춘다.
락온은 LockOnSystem이 처리한다.
막기는 PlayerController의 Block 상태와 PlayerHealth의 데미지 무시 처리로 연결되어 있다.
락온 카메라는 소프트 제한 방식으로 1차 구현된 상태다.
락온 이동 2차는 완료되었다. 아직 남은 것은 락온 대상 전환과 더 정교한 카메라 보정이다.
소스 기본값은 LockOnSystem lockRange = 12f, EnemySimpleAI detectDistance = 12f일 수 있다.
Enemy가 인식하기 전에도 Player가 먼저 락온하게 만들려면 Unity Inspector에서 LockOnSystem의 Lock Range를 EnemySimpleAI의 Detect Distance보다 크게 설정한다.
추천값은 Lock Range 25~30, Detect Distance 12 정도다.

---

## 7. 현재 스크립트

### PlayerController.cs

담당 기능:

* WASD 이동
* Left Shift 달리기
* 카메라 기준 이동 방향 계산
* 락온 중 Enemy 기준 이동 방향 계산
* Rigidbody 기반 이동
* 비락온 중 이동 방향 회전
* 락온 중 Enemy 방향 회전
* 구르기 / 구르기 무적
* 막기 / IsBlocking 공개 프로퍼티
* 1타 → 2타 → 3타 콤보
* 공격 판정 타이밍 / 검 기반 Box 히트박스
* 락온 중 공격 방향 Enemy 보정
* 락온 중 구르기 입력 방향 우선 / 무입력 후방 이탈
* `AttackStartSequence`
  → 각 공격 시작마다 증가
  → EnemySimpleAI가 콤보별 새 공격을 감지하는 기준
* CorpseShield 참조
* 시신 방패 상태 입력 제한
  → 구르기 불가
  → 막기 불가
  → 돌파 시작 불가
  → 좌클릭 공격은 시신을 소모하고 정상 시작
* 시신 방패 전용 달리기 속도 `corpseShieldRunSpeed`
  → 걷기는 기본 `moveSpeed`
  → 시신 달리기는 기본 걷기보다 빠르고 일반 `runSpeed`보다 느림
* 외부 액션 잠금 기능
  → BreakCharge와 Player 피격 HitStun에서 사용

현재 주요 구조:

* PlayerState enum
  * Normal
  * Dodge
  * Block
  * Attack

주의:

* 돌파 상태는 PlayerController가 아니라 BreakCharge의 ChargeState로 관리한다.
* 시신 방패는 PlayerState에 새 상태를 추가하지 않고 별도 CorpseShield 컴포넌트로 관리한다.
* `CanStartExternalSkill()`은 시신을 들고 있으면 false를 반환해 돌파를 막는다.
* 시신을 든 상태의 좌클릭은 공격을 막지 않는다. 먼저 시신을 소모한 뒤 `StartAttack(1)`로 전환한다.
* Rigidbody 속도는 반드시 `linearVelocity`를 사용한다.

### PlayerHealth.cs

담당 기능:

* Player HP 관리
* maxHp / currentHp 관리
* CurrentHp / MaxHp / IsDead 프로퍼티
* PlayerDamageResult 반환
  * Damaged
  * Dodged
  * Blocked
  * Cooldown
  * Invalid
* 피격 쿨다운
* 구르기 무적 판정
* 시신 방패 판정
* 우클릭 막기 판정
* 실제 피격 시 카메라 흔들림 / 넉백 / HitStun
* Heal / FullHeal
* HP 0 사망 로그

현재 TakeDamage 판정 순서:

1. 사망 / 잘못된 데미지
2. 피격 쿨다운
3. 구르기 무적
4. 시신 방패 `TryBlockAttack()`
5. 우클릭 일반 막기
6. 실제 HP 감소

시신 방패가 공격을 대신 받으면:

* 시신 파괴
* Player HP 감소 없음
* 약한 막기 카메라 흔들림
* `PlayerDamageResult.Blocked` 반환
* EnemyAttack에서는 “공격이 막힘”으로 처리

주의:

* 현재 테스트 단계에서는 Player가 사망해도 Destroy하지 않는다.
* 시신 방패는 PlayerHealth가 직접 시신을 관리하지 않고 CorpseShield에 차단 여부만 요청한다.
* PlayerHealth.TakeDamage는 bool이 아니라 PlayerDamageResult를 반환한다.

### BreakCharge.cs

담당 기능:

* 유카 고유 스킬 “돌파” 축소판
* Left Shift + Right Mouse 조합 입력 감지
* 입력 유지 중 돌파 상태 지속
* 입력 해제 시 리커버리 상태 진입
* 돌파 중 PlayerController 외부 잠금
* 돌파 중 Rigidbody 기반 전방 이동
* 돌파 중 WASD 입력이 있으면 카메라 기준 WASD 방향으로 진행 방향 갱신
* WASD 입력이 없으면 카메라 정면 방향으로 진행
* 돌파 히트박스 판정
* EnemyHitBox 우선 감지
* EnemyHealth.TakeDamage(damage, attackerPosition, knockbackMultiplier) 호출
* 돌파 중 같은 Enemy 중복 타격 방지
* 돌파 히트박스 Gizmo 시각화
* 돌파 카메라 흔들림 구현
* 돌파 명중 임시 이펙트 구현

담당 기능 추가:

* WASD 입력 기반 돌파 방향 결정
* 카메라 기준 입력 방향 계산
* 입력 방향이 있으면 해당 방향으로 돌파
* 입력 방향이 없으면 기존처럼 카메라 정면으로 돌파
* 락온 중에도 후방/좌우 돌파 가능

주의:

BreakCharge는 돌파 중 PlayerController를 외부 잠금 상태로 만들기 때문에, 돌파 중 이동 방향은 PlayerController가 아니라 BreakCharge가 직접 WASD 입력을 읽어 계산한다.

현재 주요 구조:

* ChargeState enum

  * Ready
  * Charging
  * Recovery

주의:

BreakCharge에는 `FixedUpdate()`가 반드시 있어야 한다.
돌파 시작 로그가 뜨는데 캐릭터가 굳으면, `FixedUpdate()`에서 `UpdateCharging()`이 호출되는지 먼저 확인한다.

과거 하드락 방식에서는 돌파가 cameraTransform.forward를 따라 Enemy에게 강하게 빨려 들어가는 문제가 있었다.
현재는 소프트 락온 카메라 제한을 사용하고, BreakCharge가 WASD 입력을 직접 읽어 카메라 기준 돌파 방향을 계산한다.
따라서 락온 중에도 W/S/A/D 입력에 따라 전방/후방/좌우 돌파가 가능하다.

### LockOnSystem.cs

담당 기능:

* Tab 또는 Middle Mouse 입력 감지
* 근처 Enemy 탐색
* 가장 적절한 Enemy를 currentTarget으로 지정
* 다시 입력 시 락온 해제
* Enemy 사망 시 락온 자동 해제
* 락온 인디케이터 생성/삭제
* LockOnTargetPoint 기준으로 인디케이터 위치 갱신
* 인디케이터를 카메라 쪽으로 살짝 빼서 적 몸 중앙에 박힌 느낌으로 표시

담당 기능 추가/수정:
* CameraFollow.SetLockTarget 호출 활성화
* 락온 시 CameraFollow에 currentTarget 전달
* 락온 해제 시 CameraFollow의 lockTarget 제거
* Lock Range를 Enemy detectDistance보다 크게 둘 수 있음
* Enemy가 아직 Player를 발견하지 않아도 Player가 먼저 락온 가능

주의:

소스 기본값은 LockOnSystem lockRange = 12f, EnemySimpleAI detectDistance = 12f일 수 있다.
Enemy가 인식하기 전에도 Player가 먼저 락온하게 만들려면 Unity Inspector에서 LockOnSystem의 Lock Range를 EnemySimpleAI의 Detect Distance보다 크게 설정한다.
추천값은 Lock Range 25~30, Detect Distance 12 정도다.

현재 락온 상태:

* 1차 락온 구현 완료: 대상 선택, 몸 중앙 인디케이터, 사망 시 해제, 소프트 카메라 제한
* 대상 선택 가능
* 몸 중앙 인디케이터 표시 가능
* Enemy 사망 시 인디케이터 제거 가능
* CameraFollow.SetLockTarget 호출은 활성화된 상태
* 카메라는 하드락이 아니라 소프트 락온 제한 방식으로 동작

주의:

현재 락온은 대상 선택, 몸 중앙 인디케이터 표시, 소프트 카메라 제한까지 구현된 1차 버전이다.
락온 이동 2차까지 완료했다. 아직 남은 것은 락온 대상 전환과 더 정교한 카메라 보정이다.

### EnemyHitBox.cs

담당 기능:

* Enemy의 실제 피격 판정 담당
* 부모 EnemyHealth 참조
* 공격/돌파가 감지한 데미지를 EnemyHealth로 전달
* damageMultiplier를 통해 부위별 데미지 배율 확장 가능
* 현재는 단일 캡슐형 피격 판정
* 추후 HeadHitBox, BodyHitBox, ArmHitBox, LegHitBox 등으로 확장 가능

### EnemySimpleAI.cs

담당 기능:

* Patrol: 시작 위치 주변 순찰
* AlertStare: 원거리 발견/응시
* AlertCircle: 원거리 선회
* Approach: 추격/압박
* ApproachAttack: 공격 범위까지 밀고 들어가 즉시 공격 시도
* CloseFaceOff: 근거리 대치
* CloseCircle: 근거리 선회
* Backstep: Player 공격 시작을 보고 낮은 확률로 후퇴
* Guard: Player 공격 시작을 보고 낮은 확률로 방어
* Flee: 너무 가까울 때 또는 공격 후 후퇴
* Return: 시작 위치 복귀
* DecideAfterAttack: 재대치 / 선회 / 후퇴
* PauseAI: 피격 HitStun

현재 주요 구조:

* EnemyState enum
  * Patrol
  * AlertStare
  * AlertCircle
  * Approach
  * ApproachAttack
  * CloseFaceOff
  * CloseCircle
  * Backstep
  * Guard
  * Flee
  * Return

Player 공격 반응:

* PlayerController.AttackStartSequence 변화를 감지한다.
* 새 공격을 감지하면 먼저 Backstep을 시도한다.
* Backstep을 선택하지 않았으면 Guard를 시도한다.
* Backstep
  → 확률 / 거리 / 상태 / 쿨다운 검사
  → Player 반대 방향 이동
  → Player를 계속 바라봄
  → 무적 없음
* Guard
  → 확률 / 거리 / 상태 / 쿨다운 검사
  → 이동 정지, Player 응시
  → EnemyHealth가 피해 감소 여부 처리
  → 일반 피격 또는 돌파 가드 브레이크 후 해제

공개 항목:

* `IsGuarding`
* `GuardDamageMultiplier`
* `ResolveGuardHit(bool brokenByCharge)`

주의:

* Backstep은 회피 보장 기능이 아니다. 검 판정에 닿으면 정상 피격된다.
* HitStun 중 StopMoving을 호출하면 넉백이 지워지므로 호출하지 않는다.
* Guard는 완전 무효화가 아니라 피해 감소이며 넉백과 경직을 유지한다.
* 테스트용 `startInGuardForTest`, `holdGuardForTest`는 가드 브레이크 재현에 사용한다.

### EnemyAttack.cs

담당 기능:

* Enemy 1타 공격 처리
* 공격 가능 거리 attackRange 관리
* 공격 쿨다운 attackCooldown 관리
* Windup / Active / Recovery 공격 상태 관리
* 공격 선딜 / 판정 활성 / 후딜 시간 관리
* Physics.OverlapBox 기반 공격 판정
* Player Layer에 속한 Collider 감지
* PlayerHealth.TakeDamage(attackDamage, transform.position) 호출
* PlayerDamageResult에 따라 공격 결과 로그 분리
  → Damaged: 공격 명중
  → Blocked: 공격이 막힘
  → Dodged: 공격을 구르기로 회피
  → Cooldown: 피격 쿨다운으로 무시
* 공격 중 같은 공격에서 중복 타격 방지
* 공격 판정 Gizmo 표시

현재 주요 구조:

* AttackState enum
  * Ready
  * Windup
  * Active
  * Recovery

주의:

EnemyAttack의 Player Layer는 Player 오브젝트를 끌어넣는 칸이 아니라 LayerMask다.
Player 루트 오브젝트의 Layer를 Player로 지정하고, EnemyAttack의 Player Layer에서도 Player를 체크해야 한다.
Player가 Default 레이어로 남아 있으면 Enemy 공격 로그는 나오지만 PlayerHealth.TakeDamage가 호출되지 않는다.
현재 EnemyAttack은 막기/구르기 여부를 직접 판단하지 않는다.
EnemyAttack은 공격이 닿으면 PlayerHealth.TakeDamage를 호출하고, PlayerHealth가 구르기 무적/막기 상태를 판정한다.
이제 EnemyAttack 로그는 막기/구르기/실제 피격을 구분한다.

### CameraFollow.cs

담당 기능:

* Player를 중심으로 도는 마우스 궤도 카메라
* 마우스 X/Y 입력으로 yaw/pitch 조절
* target 기준 focusPoint를 바라보도록 카메라 위치/회전 계산
* 돌파 명중 시 카메라 흔들림 처리

담당 기능 추가:

* 락온 중 소프트 카메라 제한
* lockTarget 기준으로 yaw/pitch가 너무 벗어나지 않도록 제한
* 마우스 회전 자유도는 유지
* Enemy를 화면 중앙에 강제로 박지 않고, 화면 밖으로 완전히 벗어나지 않게 하는 방식

현재 주요 값:

* Lock On Max Yaw Offset
* Lock On Max Pitch Offset
* Lock On Target Height

주의:

과거에는 하드락 방식의 CameraFollow.SetLockTarget 보정이 카메라 기준 이동과 충돌해 조작감 문제가 있었다.
현재는 CameraFollow.SetLockTarget 호출을 다시 활성화하되, Enemy를 중앙에 강제 고정하지 않고 화면 밖으로 완전히 벗어나지 않게 제한하는 소프트 락온 방식으로 운영한다.

### EnemyHealth.cs

담당 기능:

* HP / CurrentHp / MaxHp / IsDead
* TakeDamage 오버로드
* 일반 공격자 위치 기반 넉백
* 돌파 진행 방향 기반 `ApplyDirectedKnockback`
* EnemyFleeAI 일시 정지
* EnemySimpleAI HitStun
* EnemyAttack.InterruptAttack
* Guard 피해 감소
* `ignoreGuard` 처리
* 사망 중복 방지
* 캡슐 기울기 사망 연출
* 사망 연출 후 CorpseObject 전환

Guard 처리:

* 일반 공격
  → `damage * GuardDamageMultiplier`
  → 최소 1 데미지
  → Guard 해제
  → 넉백 / HitStun 유지
* `ignoreGuard = true`
  → Guard 피해 감소 미적용
  → 돌파에 방어 파괴
  → 원래 데미지 적용

사망 처리:

1. isDead 설정
2. EnemySimpleAI / EnemyAttack / EnemyFleeAI 비활성화
3. Rigidbody 정지 및 kinematic 전환
4. 몸통 Collider 비활성화
5. deathFallDuration 동안 천천히 기울기
6. deathRemainDuration 대기
7. Destroy하지 않고 CorpseObject.PrepareAsCorpse 호출

주의:

* 사망한 Enemy는 시신 방패 자원으로 남으므로 즉시 `Destroy(gameObject)` 하면 안 된다.
* 시신 투척 / 시신별 내구도 / 적 반응 분기는 현재 스코프에 없다.

### PlayerHUD.cs

담당 기능:

* Player HP Bar 갱신
* PlayerHealth.CurrentHp / MaxHp를 Slider에 반영
* HUDCanvas에 붙임
* PlayerHPBar Slider를 참조
* 현재는 매 프레임 RefreshHpBar로 단순 갱신
* Player HP Bar는 왼쪽 아래 배치
* Player HP Fill은 초록색 계열

주의:

현재는 최소 HUD이므로 애니메이션/피격 플래시/스태미나 표시 없음.
나중에 UI가 커지면 이벤트 기반 갱신으로 바꿀 수 있다.

### EnemyHealthBar.cs

담당 기능:

* Enemy 머리 위 HP Bar 갱신
* EnemyHealth.CurrentHp / MaxHp를 Slider에 반영
* World Space Canvas를 Enemy 위치 + worldOffset에 배치
* 카메라를 바라보도록 회전
* EnemyHPBarCanvas에 붙임
* EnemyHPBar Slider를 참조
* Enemy HP Fill은 붉은색 계열

주의:

EnemyHPBarCanvas는 Enemy 자식 오브젝트로 둔다.
Canvas Render Mode는 World Space로 설정한다.
Player HUD처럼 Screen Space - Overlay로 두면 머리 위 체력바가 아니라 화면 고정 UI가 된다.
일반 Enemy HP는 머리 위, 보스/락온 대상 HP는 추후 화면 하단 중앙으로 검토한다.

### EnemyFleeAI.cs

담당 기능:

* Player와의 거리 계산
* 일정 거리 안에 들어오면 Player 반대 방향으로 도망
* stopDistance 이상 멀어지면 정지
* 피격 시 PauseMovement로 잠깐 정지

현재 용도:

* 백업용/비교용 도망 AI
* EnemySimpleAI 사용 시 비활성화 권장
* 스크립트 파일은 삭제하지 않는다

---

### EnemyAttackCoordinator.cs

담당 기능:

* 다수 Enemy 공격권 중앙 관리
* `maxSimultaneousAttackers`만큼만 동시 공격 허가
* `CanRequestAttack(EnemyAttack enemyAttack)`
* `RequestAttackSlot(EnemyAttack enemyAttack)`
* `ReleaseAttackSlot(EnemyAttack enemyAttack)`
* 현재 기본값 1

연결 규칙:

* EnemyAttack.Awake에서 Coordinator 자동 탐색
* EnemyAttack.CanAttack에서 슬롯 여유 확인
* EnemyAttack.StartAttack에서 슬롯 획득
* 2타 연격이 끝나기 전에는 슬롯 유지
* EndAttack / InterruptAttack / OnDisable에서 반드시 반납

주의:

* Scene에 EnemyAttackCoordinator 오브젝트가 없으면 공격권 조율이 작동하지 않는다.
* 공격 중단이나 사망 시 슬롯이 남지 않도록 반납 경로를 유지한다.

### CorpseObject.cs

담당 기능:

* Enemy 사망 후 시신 사용 가능 상태 관리
* `IsAvailable`, `IsHeld`
* Rigidbody 정지 / 중력·충돌 간섭 차단
* 시신 Collider 비활성화
* Enemy HP Bar Canvas 숨김
* HoldPoint 자식으로 부착
* 공격 전환 또는 방어 소모 시 시신 파괴

주의:

* CorpseObject는 적 AI가 아니라 사망 후 전투 자원 상태다.
* 시신을 다시 살아 있는 Enemy 판정으로 잡지 않도록 Collider와 락온 후보를 비활성화한다.

### CorpseShield.cs

담당 기능:

* 가까운 CorpseObject 탐색
* E 입력으로 시신 집기
* CorpseShieldHoldPoint 자동 생성 가능
* 시신을 Player 전방에 고정
* `IsHoldingCorpse` / `HasCorpse`
* `TryBlockAttack()`
  → Enemy 공격 1회 대신 받기
  → 시신 파괴
* `ConsumeForPlayerAttack()`
  → 좌클릭 공격 전환 시 시신 포기
* 시신 방패 해제/폐기
* 들고 있는 동안 위치가 밀리지 않도록 HoldPoint 기준 고정

현재 전투 규칙:

* 이동 가능
* 걷기 속도 유지
* 전용 달리기 가능
* 구르기 / 막기 / 돌파 불가
* 좌클릭 공격 가능, 단 시신 즉시 소모
* 피격 시 시신이 대신 맞고 소모

주의:

* 현재는 시신 투척, 내구도, 적별 반응, 절단, 여러 시신 보유를 구현하지 않는다.
* 해당 기능은 완전판 Vertical Slice 후보로 남긴다.

## 8. 현재 디버깅 이력

해결된 문제:

### Input System 충돌

* `Input.GetAxisRaw` 사용 시 Input System 설정 충돌 발생
  → Active Input Handling을 Both로 변경

### Input Manager deprecation 경고

* Unity 콘솔에 `This project uses Input Manager, which is marked for deprecation` 경고 발생
  → 현재는 에러가 아니라 경고
  → 프로젝트는 Old Input Manager 방식의 `Input.GetKey`, `Input.GetMouseButton`, `Input.GetAxis`를 사용 중
  → 지금은 프로토타입 진행을 우선하고, New Input System 이전은 나중에 검토

### 카메라 떨림

* 카메라가 덜덜 떨림
  → Ground에 Rigidbody가 붙어 있었음
  → Ground Rigidbody 제거로 해결

### GitHub 파일 폭증

* GitHub에 6만 개 파일이 잡힘
  → Library 폴더가 Git에 포함되려 했음
  → .gitignore 적용 후 정상화
  → 정상 커밋 파일 수는 약 153개였음

### Player 충돌 회전

* Enemy와 충돌 시 Player가 Y축으로 마음대로 회전함
  → Rigidbody 충돌로 angular velocity가 생기는 문제
  → FixedUpdate에서 `rb.angularVelocity = Vector3.zero` 처리하여 해결

### 콤보 미작동

* 콤보가 1타만 반복되고 2타/3타가 이어지지 않음
  → 기존에는 `stateTimer <= comboInputOpenTime` 조건이 너무 빡빡했음
  → 공격 지속시간을 늘리고, 공격 중 입력 예약을 더 관대하게 처리하여 해결

### EnemyFleeAI 컴포넌트 추가 실패

* EnemyFleeAI 컴포넌트 추가 실패
  → Assets 안에 임의로 넣은 미완성 스킬 코드가 컴파일 에러를 유발
  → Unity는 Assets 안의 C# 파일을 전부 컴파일하므로, 미완성 아이디어 코드는 .md 문서나 Assets 밖에 보관해야 함
  → 컴파일 에러 제거 후 EnemyFleeAI 정상 부착

### 검 에셋 핑크색 표시

* 검 에셋이 핑크색으로 표시됨
  → 머티리얼/셰이더 문제
  → 머티리얼 수정으로 정상 표시 해결

### 검 기반 히트박스가 너무 빡빡함

* 검 기반 히트박스가 구형 판정보다 맞히기 어려움
  → 검처럼 얇은 판정은 너무 빡빡함
  → 실제 게임에서는 검보다 약간 두꺼운 몽둥이형 판정이 필요함
  → `weaponHitBoxHalfExtents` 값을 조정해 검보다 관대한 판정으로 맞추는 방향

### 공격 판정 타이밍 체감 약함

* 공격 판정 타이밍 체감이 약함
  → 애니메이션이 아직 없기 때문에 선딜/활성/후딜이 눈으로 잘 느껴지지 않음
  → 코드상으로는 공격 판정 시작/종료 시간이 분리되어 있음
  → 추후 애니메이션과 연결하면 체감이 살아날 예정

### 구르기 무적과 Enemy 공격 연결

* 구르기 무적은 EnemyAttack / PlayerHealth와 실제로 연결됨
  → Dodge 상태 중 특정 시간 동안 `IsInvincible`이 true
  → EnemyAttack이 PlayerHealth.TakeDamage를 호출했을 때 IsInvincible이면 데미지 무시
  → 테스트 중 “구르기 무적 중 - 데미지 무시” 로그 확인 완료

### 돌파 구현 중 입력 충돌

* Left Shift + Right Mouse 입력 시 캐릭터가 굳는 문제 발생
  → PlayerController가 Shift+우클릭 입력을 return으로 먹어버리거나, BreakCharge가 시작 후 갱신되지 않는 문제가 있었음
  → PlayerController는 기본 우클릭 막기를 유지하고, BreakCharge가 입력 조합을 감지하면 외부 잠금을 거는 방식으로 정리
  → BreakCharge에 `FixedUpdate()`가 빠져 있어 돌파 시작 후 `UpdateCharging()`이 호출되지 않았음
  → `FixedUpdate()` 추가 후 돌파 정상 작동

### 돌파 입력 순서 문제

* 처음에는 Left Shift 입력 후 Right Mouse를 눌러야 돌파가 발동하는 문제가 있었음
  → 입력 순서와 상관없이 `Left Shift + Right Mouse`가 함께 유지되면 돌파가 시작되도록 변경
  → 우클릭 막기 중 Shift 추가 입력으로도 돌파 가능
  → Shift 달리기 중 우클릭 추가 입력으로도 돌파 가능

### 돌파 방향 고정 문제

* 처음에는 돌파 시작 순간의 방향으로만 계속 진행됨
  → 돌파 중 마우스 카메라 방향을 따라 진행 방향을 갱신하도록 변경
  → 현재는 돌파 중에도 마우스로 방향 전환 가능

### EnemyHitBox 분리

* 기존에는 Enemy 본체 Capsule Collider가 물리 몸통이면서 동시에 피격 판정 역할도 했다.
  → Enemy 본체와 피격 판정을 분리하기 위해 EnemyHitBox 자식 오브젝트 추가
  → Enemy 본체 Capsule Collider는 물리 충돌/이동용으로 유지
  → EnemyHitBox 자식 Capsule Collider는 Is Trigger를 체크하고 피격 판정 전용으로 사용

* Player 공격과 돌파가 EnemyHealth를 직접 찾던 구조에서 EnemyHitBox를 우선 감지하는 구조로 변경
  → EnemyHitBox가 있으면 EnemyHitBox를 통해 부모 EnemyHealth에 데미지 전달
  → EnemyHitBox가 없으면 기존처럼 EnemyHealth를 fallback으로 찾음
  → 기존 Enemy 구조와 새 EnemyHitBox 구조가 모두 작동하도록 안전장치 유지

* 히트박스 분리의 목적
  → 물리 몸통과 피격 판정을 따로 조절 가능
  → 추후 부위별 판정, 약점 판정, 무장해제 판정, 락온 중심점, 타겟 인디케이터와 연결 가능

### 락온 인디케이터 위치 문제

* 처음에는 락온 인디케이터가 Enemy 머리 위에 떠서, 액션 게임식 락온 느낌이 약했다.
  → Enemy 자식으로 LockOnTargetPoint를 만들고, 복부/가슴 근처에 배치
  → LockOnSystem이 LockOnTargetPoint를 우선 기준점으로 사용
  → 인디케이터를 카메라 쪽으로 약간 빼서 몸 중앙에 박힌 느낌으로 보이게 조정

### Enemy 사망 후 락온 인디케이터 잔류 문제

* 락온 상태에서 Enemy를 처치하면 인디케이터가 사라지지 않는 문제 발생
  → currentTarget이 사라져도 currentIndicator가 남아 있으면 UpdateCurrentTarget을 돌게 수정
  → currentTarget이 null이면 Unlock 호출
  → Enemy 사망 시 인디케이터 제거 정상화

### 락온 카메라 보정 조작감 문제

* 락온 시 CameraFollow가 Enemy를 강제로 바라보게 하자 이동이 이상해짐
  → Player 이동은 카메라 기준 이동이므로, 카메라가 Enemy를 바라보면 W 입력이 Enemy 쪽 돌진처럼 작동
  → A/D/S도 자유 이동이 아니라 묶인 느낌 발생
  → 돌파도 cameraTransform.forward를 따라가므로 Enemy에게 강하게 빨려 들어가는 느낌 발생
  → 이후 CameraFollow.SetLockTarget 호출을 다시 활성화하되, 하드락이 아니라 소프트 락온 제한 방식으로 변경
  → 현재 1차 락온은 대상 선택, 인디케이터 표시, 소프트 카메라 제한까지 구현된 상태

### EnemySimpleAI 1차 구현 / 과거 구조

* 기존 EnemyFleeAI는 도망 행동만 있었다.
  → EnemySimpleAI를 새로 만들어 공격 전 단계의 기본 상태머신 구현
  → Patrol / FaceOff / Approach / Flee / Return 상태 추가
  → Player 발견 전 순찰, 발견 후 대치, 거리별 접근/후퇴, 멀어지면 복귀까지 구현
  → Enemy 공격은 Player HP 미구현 상태이므로 다음 단계로 보류
  → 이후 현재 구조는 Patrol / AlertStare / AlertCircle / Approach / CloseFaceOff / CloseCircle / Flee / Return으로 재설계됨

### EnemySimpleAI 대치 구조 재설계

* 기존 EnemySimpleAI는 발견 후 너무 빠르게 Approach로 넘어가는 문제가 있었다.
  → AlertStare를 단순 대기 상태가 아니라 원거리 발견/응시 상태로 재정의
  → AlertCircle을 추가해 원거리에서 Player를 바라보며 선회 가능하게 수정
  → Approach는 원거리 대치 이후 Player가 가까이 들어왔을 때 발동하는 추격/압박 상태로 재정의
  → CloseFaceOff / CloseCircle을 추가해 근거리 대치 구조를 분리
  → Enemy가 바로 달려드는 느낌보다, 보고/간보고/압박하고/근거리에서 다시 대치하는 흐름으로 변경

### 락온 카메라 소프트 제한
* 기존 락온은 카메라 보정이 꺼져 있어서 마우스를 돌리면 Enemy가 화면 밖으로 사라질 수 있었다.
  → CameraFollow에 소프트 락온 제한 추가
  → 락온 대상 기준으로 yaw/pitch가 일정 범위 이상 벗어나지 않도록 제한
  → Enemy를 중앙에 강제로 고정하지 않고, 화면 안에 남도록만 제한
  → 기존 하드락 방식보다 카메라 기준 이동/돌파와 충돌이 줄어듦

### 돌파 방향 문제
* 락온 중 S + Left Shift + Right Mouse를 눌러도 Enemy 방향으로 돌파하는 문제가 있었다.
  → 원인은 BreakCharge가 WASD 입력을 보지 않고 cameraTransform.forward만 기준으로 돌파 방향을 계산했기 때문
  → BreakCharge가 직접 WASD 입력을 읽어 카메라 기준 입력 방향을 계산하도록 수정
  → W/S/A/D 입력에 따라 전방/후방/좌우 돌파 가능
  → 입력이 없을 때만 카메라 정면 돌파 유지

### 락온 거리 조정
* Enemy가 Player를 인식하기 전에도 Player가 먼저 락온할 수 있게 하고 싶었다.
  → LockOnSystem의 lockRange를 EnemySimpleAI의 detectDistance보다 크게 설정
  → Enemy 인식 거리와 Player 락온 거리를 분리
  → 다크소울식으로 “적이 날 몰라도 나는 먼저 락온 가능”한 구조로 조정

### PlayerHealth / EnemyAttack 1차 공방 구현

* PlayerHealth.cs 추가
  → Player HP, 피격, 회복, 사망 로그 관리
  → HP가 0이 되어도 테스트 편의를 위해 Destroy하지 않음
  → 구르기 무적 / 막기 상태에서 데미지를 무시하도록 연결

* EnemyAttack.cs 추가
  → Enemy 공격 선딜 / 판정 활성 / 후딜 구조 구현
  → EnemySimpleAI의 CloseFaceOff / CloseCircle에서 EnemyAttack 호출
  → Enemy 공격 판정이 Player를 감지하면 PlayerHealth.TakeDamage 호출
  → 현재 Enemy 공격은 단일 1타 공격이며 데미지는 약하게 운용

### EnemyAttack Player Layer 설정 문제

* EnemyAttack의 Player Layer를 Player로 설정했는데도 PlayerHealth가 줄지 않는 문제 발생
  → 원인은 Player 루트 오브젝트의 Layer가 Default로 남아 있었기 때문
  → EnemyAttack의 Player Layer는 LayerMask이므로, 실제 Player 오브젝트 Layer도 Player로 지정해야 함
  → Player 루트의 Layer를 Player로 변경하고 children에도 적용하자 피격 로그 정상 출력

### 막기 데미지 무시 연결

* 우클릭 막기는 기존에는 이동 속도 감소만 체감되고 실제 방어 효과가 없었다.
  → PlayerController에 IsBlocking 공개 프로퍼티 추가
  → PlayerHealth.TakeDamage에서 IsBlocking이면 데미지 무시
  → 막기 중 Enemy 공격을 맞으면 “막기 성공 - 데미지 무시” 로그 출력
  → 현재는 일반 Enemy 공격을 막으면 데미지 0
  → 아직 막기 이펙트, 충격 정지, 가드 브레이크, 스태미나는 없음

### EnemyAttack 로그 표현 주의

* 막기 성공 시에도 EnemyAttack 쪽에서 “Enemy 공격 명중! 데미지: 10” 로그가 출력될 수 있다.
  → 이유는 EnemyAttack은 공격 판정이 Player에게 닿았다는 사실만 알고, 실제 HP 감소 여부는 PlayerHealth가 판단하기 때문
  → 실제 데미지 적용 여부는 PlayerHealth의 “Player 피격”, “구르기 무적 중 - 데미지 무시”, “막기 성공 - 데미지 무시” 로그로 확인한다.
  → 추후 PlayerHealth.TakeDamage가 bool을 반환하도록 바꾸면 명중/막힘/무적 로그를 더 정확히 분리할 수 있다.

### 막기 피드백 / 공격 결과 로그 분리

* 기존에는 막기 성공 시 PlayerHealth가 데미지를 무시해도 EnemyAttack이 “공격 명중! 데미지: 10”을 출력해 헷갈렸다.
  → PlayerHealth.TakeDamage를 bool 반환으로 바꾸었다가, 다시 PlayerDamageResult enum 반환으로 확장
  → 막기/구르기/피격쿨다운/실제피격을 EnemyAttack 로그에서 구분 가능하게 수정
  → 막기 성공 시 “공격이 막힘”
  → 구르기 무적 성공 시 “공격을 구르기로 회피”
  → 실제 피격 시 “공격 명중! 데미지: N”

### Player 피격 피드백 강화

* 실제 피격 시 Console 로그만 뜨고 손맛이 약했다.
  → PlayerHealth에서 실제 HP 감소 시 카메라 흔들림 추가
  → attackerPosition 반대 방향으로 Player 넉백 추가
  → 짧은 hitStunDuration 동안 PlayerController 외부 액션 잠금
  → 막기/구르기 성공 시에는 피격 넉백이 발생하지 않음
  → 막기 흔들림보다 피격 흔들림이 더 강하게 느껴지도록 별도 수치 분리

### HP UI / HUD 구현

* Console로만 Player/Enemy HP를 확인하던 상태에서 HUD를 추가했다.
  → PlayerHUD.cs 추가
  → Player HP Bar를 화면 왼쪽 아래에 표시
  → Player HP Fill은 초록색
  → EnemyHealthBar.cs 추가
  → Enemy 머리 위 World Space Canvas에 Enemy HP Bar 표시
  → Enemy HP Fill은 붉은색
  → Canvas와 Slider의 역할을 구분
     - Canvas: UI를 올려놓는 판
     - Slider: 체력바로 사용하는 실제 UI 부품
  → Player가 맞으면 Player HP Bar가 줄고, 막기/구르기 성공 시 줄지 않는 것 확인
  → Enemy를 때리면 Enemy 머리 위 HP Bar가 줄어드는 것 확인

### 기본 combat prototype 1차 루프 완료

* 현재 최소 전투 루프는 성립했다.
  → Player 이동
  → Player 공격
  → Enemy 피격/사망
  → Enemy AI 접근/대치
  → Enemy 공격
  → Player 피격
  → Player 구르기 회피
  → Player 막기 방어
  → Player/Enemy HP UI 표시
  → 피격/막기/회피 로그 분리
  → 피격 넉백과 카메라 흔들림

---

### Enemy 피격 / 사망 연출 강화

* Enemy가 HP 0에서 즉시 삭제되어 사망이 눈에 읽히지 않았다.
  → AI와 공격을 정지하고 캡슐을 천천히 기울이는 DeathFallRoutine 추가
  → 캡슐 Pivot이 중앙이라 바닥에 박히는 문제는 deathLowerDistance로 함께 보정
* 시신 방패 기능을 위해 사망 후 Destroy를 제거했다.
  → 사망 연출 종료 후 CorpseObject로 전환
  → HP Bar와 피격 Collider를 비활성화

### Enemy 백스텝 감지 문제

* 처음에는 PlayerController.IsAttacking bool로 공격 시작을 감지했다.
  → 콤보 사이에 상태가 계속 true로 보일 수 있어 새 공격을 놓침
  → PlayerController.AttackStartSequence 도입
  → 각 공격 시작마다 번호가 증가하고 Enemy가 번호 변화를 감지
* 백스텝 로그는 나오지만 움직임이 작아 보였다.
  → 코드 문제가 아니라 Inspector의 backstepSpeed / backstepDuration 수치가 너무 작았음
  → 테스트 시 확률 1, 속도/지속시간을 높여 기능 확인 후 실전값으로 복귀
* 백스텝에는 무적이 없다.
  → 늦게 반응하면 맞으면서 뒤로 물러날 수 있음

### Enemy Guard / 돌파 가드 브레이크

* Guard 상태 진입과 실제 데미지 감소를 EnemySimpleAI만으로 처리할 수 없었다.
  → EnemyHealth에 Guard 확인과 ignoreGuard 오버로드 추가
  → EnemyHitBox에도 ignoreGuard 전달 오버로드 추가
* 일반 공격 20이 Guard에서 10으로 감소하는 것 확인
* 돌파 40은 Guard를 깨고 HP 100 → 60으로 적용되는 것 확인
* 테스트 편의를 위해 startInGuardForTest / holdGuardForTest 옵션 추가

### 시신 방패

* 사망한 Enemy를 Player 앞에 붙였지만 처음에는 Player 이동을 따라오지 않았다.
  → 시신 Rigidbody를 kinematic 처리하고 충돌/중력 간섭 제거
  → HoldPoint 자식 부착
  → LateUpdate에서 localPosition / localRotation 재고정
* 시신 방패가 모든 행동을 막으면 유카 액션의 속도감과 공격성이 죽었다.
  → 방어적 입력만 제한
  → 구르기 / 막기 / 돌파 금지
  → 좌클릭 공격은 시신을 소모하고 즉시 공격 전환
* 시신을 들어도 걷기 속도는 유지한다.
  → Shift 달리기는 평상시 달리기보다 느리지만 걷기보다 빠른 corpseShieldRunSpeed 사용
* Enemy 공격을 받으면 시신이 1회를 대신 받고 Player HP는 감소하지 않는 것 확인

### 캡슐 프로토타입 코드 스코프 종료

* 2026-05-26 개발 시작
* 2026-06-13 코드 기능 범위 종료
* 경과 18일 / 양끝 날짜 포함 19일
* 이후 새 시스템을 계속 추가하지 않고 에셋·애니메이션·연출 단계로 이동
* 마법 해제는 적 마법 시스템이 생기는 이후 단계로 보류

## 9. GitHub 상태

저장소 이름:
`combat_prototype`

개발 루틴:

1. 작업 전 GitHub Desktop에서 Fetch origin
2. Unity 작업
3. Ctrl + S 저장
4. GitHub Desktop에서 변경 파일 확인
5. Summary 작성
6. Commit to main
7. Push origin

커밋은 기능 단위로 자주 남긴다.

예:

* Fix combo and collision rotation
* Add enemy flee AI and knockback
* Add temporary sword model to player
* Add sword-based attack hitbox
* Add attack hit timing window and range visualization
* Add dodge invincibility window
* Add prototype charge break skill
* Add enemy hitbox component
* Add basic lock on system
* Add basic enemy state AI

주의:

* Commit만 하면 로컬 저장이다.
* Push origin까지 해야 GitHub 웹 저장소도 최신화된다.
* 작업 전 Fetch origin을 눌러 원격 저장소 상태를 확인한다.
* Unity 프로젝트의 Library, Temp, Logs, UserSettings 등은 Git에 올리지 않는다.
* 기능 하나를 끝내면 인수인계서도 같이 갱신한다.

최근 작업 커밋 후보:

* Complete basic combat loop and HUD

---

## 10. 현재 완료 상태와 다음 단계

### 캡슐 프로토타입 완료 판단

현재 코드는 “기본 골격”을 넘어, 유카 고유 전투 정체성을 확인할 수 있는 캡슐 프로토타입 완료 상태로 본다.

완료된 전투 루프:

* Player 이동 / 달리기
* 궤도 카메라
* 락온 대상 선택 / 인디케이터 / 소프트 카메라 제한
* 락온 W/S 접근·후퇴 / A/D 선회
* 3타 콤보
* 검 기반 Box 히트박스
* 구르기 / 구르기 무적
* 우클릭 막기
* Player 피격 / 넉백 / HitStun / 카메라 흔들림
* Player / Enemy HP UI
* Enemy 순찰 / 응시 / 원거리 선회 / 접근 / 근거리 대치 / 선회 / 후퇴
* Enemy 1타 / 확률적 2타 연격
* 공격 후 재대치 / 선회 / 후퇴
* 다수 Enemy 공격권 조율
* Enemy 피격 / 넉백 / 공격 중단 / HitStun
* Enemy 제한적 백스텝
* Enemy 제한적 Guard
* 돌파 가드 브레이크
* Enemy 사망 기울기 연출
* 시신 전환
* 유카 고유 스킬 돌파
* 유카 고유 스킬 시신 방패 축소판

### 현재부터 보류하는 코드 기능

* 마법 해제
  → 적 마법 / 마법 구조체 / 투사체 시스템과 함께 구현
* 부위별 실제 HitBox 배치
  → 인간형 모델 적용 후 Head / Body / Arm / Leg 배치
* 시신 방패 2차
  → 적 반응: 기겁 / 움찔 / 무시 / 시신째 베기
  → 시신 투척
  → 투척 충돌 데미지 / 넉백
  → 시신 절단 / 적별 내성
* 락온 대상 전환
* 더 정교한 카메라 중앙 보정
* 패링 / 받아묶기 / 무장해제 / 자동패링
* 적 마법사 / 원거리 투사체
* 스태미나 / 가드 브레이크 / 강공격
* 래그돌 / 시체 물리 / 영구 시체 관리

### 다음 실제 개발 단계

1. 현재 상태 전체 회귀 테스트
2. `Complete capsule combat prototype` 커밋 및 Push
3. 캡슐 프로토타입 보존용 브랜치 생성
   → 학기 팀플 / 캡스톤 / 구조 증명용 보험
4. 메인 개발은 기본 취업 포트폴리오 데모 목표로 진행
5. 캐릭터 / Enemy / 애니메이션 / 이펙트 / 사운드 에셋 조사
6. 에셋에 맞춰 기존 수치 재조정
   → 공격 선딜 / 활성 / 후딜
   → 히트박스 위치
   → 넉백 / 경직
   → Enemy 거리 / 이동속도 / 백스텝 / Guard 타이밍
7. 최소 스토리 UI와 짧은 진행 구조 추가
8. 플레이 영상 / README / 기획 의도 / 사용 에셋 라이선스 정리

### 목표 구분

* 캡슐 프로토타입
  → 전투 구조 증명
  → 학기 팀플 / 캡스톤 보험
* 기본 취업 포트폴리오 데모
  → 에셋, 애니메이션, UI, 짧은 스토리 진행까지 포함
* 완전판 Vertical Slice
  → 추후 취준 본격화 또는 IP 반응 발생 시 별도 확장
  → 시신 투척 / 적 반응 분기 / 마법 해제 등 고유 전투 시스템 2차 구현

현재 공식 목표는 “기본 취업 포트폴리오 데모”다.
완전판 Vertical Slice는 지금 당장 만들지 않는다.

## 11. 새 AI에게 주의시킬 것

현재 구조를 함부로 갈아엎지 말 것.
초보자용이라도 기초 문법 강의로 돌리지 말 것.
작은 기능 단위로 진행할 것.
한 번에 여러 시스템을 만들지 말 것.
코드를 줄 때는 기존 PlayerController 구조와 충돌하지 않게 전체 교체인지 부분 교체인지 명확히 말할 것.

특히 주의:

* Rigidbody 속도는 `rb.velocity`가 아니라 `rb.linearVelocity` 사용
* `Input.GetAxisRaw("Horizontal")` 기반 이동 입력은 피할 것
* Ground에는 Rigidbody를 붙이지 말 것
* Assets 안에는 컴파일되지 않는 미완성 C# 코드 조각을 넣지 말 것
* 현재 입력은 Old Input Manager 기반이며, Input Manager deprecation 경고는 현재 무시 가능
* 현재 공격 판정은 구형 AttackPoint 방식이 아니라 검 기반 Box 히트박스 방식으로 전환 중
* 완벽한 검날 판정보다, 조작감 좋은 두꺼운 판정을 우선한다
* 구르기 무적은 EnemyAttack / PlayerHealth와 연결되어 있으며, 무적 중 데미지를 무시한다
* PlayerHealth는 추가되었고, Player HP / 피격 / 사망 로그를 관리한다
* EnemyAttack은 추가되었고, Enemy의 1타 공격과 Player 피격을 처리한다
* 돌파는 PlayerController가 아니라 BreakCharge.cs에서 관리한다
* 돌파 중 PlayerController는 외부 액션 잠금 상태가 된다
* 돌파가 “시작 로그만 뜨고 굳으면” BreakCharge.FixedUpdate가 있는지 먼저 확인한다
* EnemyHitBox 분리 완료
* Enemy 본체 Capsule Collider는 물리 충돌/이동 몸통 역할
* EnemyHitBox 자식 Collider는 피격 판정 역할
* EnemyHitBox Collider는 Is Trigger를 체크해야 한다
* EnemyHitBox 오브젝트의 Layer는 Enemy로 설정한다
* Player 루트 오브젝트의 Layer는 Player로 설정한다
* EnemyAttack의 Player Layer도 Player로 체크한다
* 공격/돌파 판정은 EnemyHitBox를 우선 감지하고, 없으면 EnemyHealth를 fallback으로 찾는다
* LockOnTargetPoint는 락온 인디케이터 기준점이다
* 락온 인디케이터는 Enemy 몸 중앙/가슴 근처에 위치시키는 것이 목표다
* 현재 락온은 대상 선택 + 인디케이터 표시 + 소프트 카메라 제한까지 구현된 1차 버전이다
* CameraFollow.SetLockTarget 호출은 현재 다시 활성화되어 있다.
* 다만 하드락 방식이 아니라, Enemy가 화면 밖으로 완전히 벗어나지 않게 제한하는 소프트 락온 방식이다.
* 락온 이동 2차는 PlayerController에 연결되었다. W/S 접근·후퇴, A/D 선회, Enemy 방향 바라보기, 공격 방향 보정이 작동한다.
* EnemyFleeAI는 삭제하지 말고 백업용으로 유지한다
* EnemySimpleAI를 사용할 때는 EnemyFleeAI 컴포넌트를 비활성화한다
* EnemySimpleAI는 Patrol / AlertStare / AlertCircle / Approach / CloseFaceOff / CloseCircle / Flee / Return 구조다.
* EnemySimpleAI는 직접 공격 판정을 처리하지 않고 EnemyAttack에게 공격 시작을 명령한다
* EnemyHealth는 아직 EnemySimpleAI의 HitStun과 직접 연결되어 있지 않다
* 새로운 기능을 넣기 전 현재 Play 테스트로 이동/공격/콤보/Enemy 피격/넉백/돌파/락온/EnemySimpleAI가 정상인지 확인할 것
* 새 기능 성공 후 인수인계서와 스크립트 파일을 최신화할 것
* EnemySimpleAI의 현재 핵심은 원거리 대치와 근거리 대치를 나누는 것이다.
* AlertStare는 원거리 발견/응시 상태다.
* AlertCircle은 원거리에서 Player를 바라보며 선회하는 상태다.
* Approach는 Player가 가까이 들어왔을 때 추격/압박하는 상태다.
* CloseFaceOff는 근거리 대치 상태다.
* CloseCircle은 근거리 선회 상태다.
* Flee는 너무 가까워졌을 때 후퇴 상태다.
* Enemy 공격은 현재 CloseFaceOff / CloseCircle 쪽에서 EnemyAttack을 호출하는 방식으로 연결되어 있다.
* LockOnSystem의 lockRange는 EnemySimpleAI의 detectDistance보다 커도 된다.
* Player는 Enemy가 아직 인식하지 않아도 먼저 락온할 수 있다.
* CameraFollow는 락온 중 소프트 카메라 제한을 사용한다.
* 락온 카메라는 Enemy를 중앙에 강제 고정하지 않고, 화면 밖으로 완전히 벗어나지 않게 제한하는 방식이다.
* BreakCharge는 WASD 입력을 직접 읽어 카메라 기준 돌파 방향을 계산한다.
* 락온 중 돌파는 W/S/A/D 입력에 따라 전방/후방/좌우 기동이 가능해야 한다.
* 돌파 방향을 다시 cameraTransform.forward 고정 방식으로 되돌리면 안 된다.
* 적 공격 데미지는 즉사급으로 만들지 않는다. 이것은 PlayerHealth의 데미지 상한 기능이 아니라 전투 기획 규칙이다.
* 현재 막기 성공 시 데미지는 0으로 처리한다. 나중에 강공격/가드 브레이크/마법 공격에서 예외를 만들 수 있다.
* PlayerHealth.TakeDamage는 PlayerDamageResult를 반환한다.
* EnemyAttack은 PlayerDamageResult에 따라 실제 피격/막기/구르기/쿨다운 로그를 분리한다.
* Player HUD는 HUDCanvas의 PlayerHUD.cs가 관리한다.
* Enemy 머리 위 HP Bar는 EnemyHPBarCanvas의 EnemyHealthBar.cs가 관리한다.
* EnemyHPBarCanvas는 World Space Canvas여야 한다.
* 일반 Enemy HP는 머리 위, 보스/락온 대상 HP는 추후 화면 하단 중앙으로 검토한다.
* 내일 다음 작업은 Enemy 공격 패턴 2차 구현이 우선이다.
* 목표는 단순히 어려운 적이 아니라, “이 새끼가 그걸 내 앞에서 한다고?”라는 감각을 주는 잡몹 행동 분기다.


* EnemySimpleAI에는 Backstep / Guard 상태가 추가되었다.
* PlayerController.AttackStartSequence는 Enemy가 각 콤보 공격 시작을 감지하는 기준이다. 제거하거나 bool 감지로 되돌리지 않는다.
* Backstep에는 무적이 없다. 실제 검 판정을 벗어나야 회피 성공이다.
* Guard는 완전 방어가 아니라 피해 감소다. 넉백과 HitStun은 유지한다.
* BreakCharge는 EnemyHealth / EnemyHitBox의 ignoreGuard 오버로드를 true로 호출해 Guard를 깨야 한다.
* Enemy 사망 시 Destroy하지 않는다. DeathFallRoutine 종료 후 CorpseObject로 전환한다.
* LockOnSystem은 EnemyHealth.IsDead가 true인 대상은 후보에서 제외하고 기존 락온도 해제해야 한다.
* Player에는 CorpseShield 컴포넌트가 있어야 한다.
* CorpseShieldHoldPoint는 자동 생성하거나 Player 자식으로 유지한다.
* 시신 방패 중 구르기 / 일반 막기 / 돌파는 불가하다.
* 시신 방패 중 좌클릭 공격은 가능하지만, 먼저 시신을 소모한 뒤 공격을 시작한다.
* 시신 방패 중 걷기 속도는 유지하고, Shift는 일반 달리기보다 느린 corpseShieldRunSpeed를 사용한다.
* PlayerHealth.TakeDamage 판정 순서는 피격 쿨다운 → 구르기 → 시신 방패 → 일반 막기 → 실제 피해다.
* 시신 방패가 공격을 대신 받으면 PlayerDamageResult.Blocked를 반환한다.
* 마법 해제는 현재 코드 스코프에서 제외했다. 적 마법 시스템 없이 고립된 삭제 기능으로 먼저 만들지 않는다.
* 캡슐 프로토타입의 새 코드 기능 추가는 종료했다. 다음 단계는 에셋 / 애니메이션 / 연출 / UI다.
* 현재 저장소 이름은 `combat_prototype`이다.

작업 시작 루틴:

1. GitHub Desktop에서 Fetch origin
2. Unity Play 테스트
3. 인수인계서에서 현재 버그/다음 목표 확인
4. 기능 하나만 작업
5. 성공 확인
6. 인수인계서 최신화
7. Commit + Push
