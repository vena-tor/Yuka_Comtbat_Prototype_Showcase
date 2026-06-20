# combat_prototype 에셋·애니메이션 인수인계서

> 이 문서는 `전투시스템_인수인계서.md`의 후속 문서다.  
> 전투 코드의 현재 상태와 판정 규칙은 기존 코드 인수인계서를 기준으로 하며, 이 문서는 **모델·리깅·Animator·애니메이션·VFX·사운드·맵 에셋 적용 과정**만 관리한다.
>
> 이 문서에서는 **0번, 8번, 10번, 11번, 13번 항목을 최신 상태의 기준**으로 본다.  
> 과거 시행착오는 7번 디버깅 이력에 남기되, 현재 작업 판단은 최신 항목을 우선한다.

---

## 0. 최신 갱신 요약 (2026-06-20 Unity 3D 액션 전투 프로토타입 종료)

개발 시작일은 **2026-05-26**, Unity 작업 종료일은 **2026-06-20**이다.  
25일 동안 캡슐 전투 코드에서 시작해 인간형 Player와 Enemy, 애니메이션, 락온, 시신 방패, 최소 야외 맵까지 연결된 3D 액션 전투 프로토타입을 완성했다.

캡슐 전투 프로토타입의 코드 기능 범위는 2026-06-13에 종료했고, 에셋 통합은 2026-06-14~20에 진행했다.  
2026-06-20 기준으로 **Unity에서 정한 현재 스코프는 전부 닫혔다.**

### 최종 완료 범위

#### Player

* 인간형 모델 / Humanoid Avatar / URP 머티리얼
* Idle / Walk 8방향 / Sprint
* 대검 공격 3타와 실제 판정 시간 연결
* 자유 방향 Dodge와 무적 구간
* 일반 BlockLocomotion / BlockHit
* BreakCharge와 전용 검 외형
* 일반 Hit
* CorpseShieldLocomotion / CorpseRun
* 인간형 시신 방패의 최종 잡기 위치와 실루엣

#### Enemy

* 검방 인간형 모델
* Combat Idle / 전진 / 후진 / 좌우 선회
* Patrol / Alert / Approach / CloseFaceOff / CloseCircle / Flee / Return
* 공격 1타 / 2타와 개별 판정 시간
* 제자리 Guard / Guarded CloseCircle
* 일반 Hit / 돌파 전용 HeavyHit / Death
* 공격 중 피격 시 공격 중단
* 사망 후 동일 오브젝트의 CorpseObject 전환

#### 공통 전투 / UI

* 인간형 Player와 Enemy의 1:1 전투 루프
* Screen Space 락온 마커
* Player / Enemy HP UI
* Guard / Guard Break
* 일반 피격 / 강피격 / 사망 결과 구분
* 시신 방패 1회 방어와 공격 전환 소모
* Kinematic Rigidbody 속도 경고 제거

#### 맵

* 기존 `Ground`를 실제 물리 바닥으로 유지
* Ground에 녹색 URP/Lit 머티리얼 적용
* PolyTope Studio 자연환경 에셋을 `MapVisualRoot` 아래 배치
* 나무 / 관목 / 바위 / 풀을 Cluster 단위로 묶어 복제
* Cluster마다 Rotation Y와 Scale을 바꿔 반복감을 줄임
* 중앙 전투 공간을 비우고 외곽에 자연물을 배치한 소형 야외 전투장 완성

### 2026-06-20 완료: 인간형 시신 방패 정렬

시신 루트 피벗을 Player 손에 직접 붙이는 기존 방식은 골반이나 가랑이를 잡은 것처럼 보이고, 손 뼈의 Scale을 상속받아 시신이 비대해지는 문제가 있었다.

최종 구조는 잡는 위치와 몸 전체 방향을 분리한다.

```text
Player
├─ CorpseShieldPoseAnchor
└─ PlayerVisualRoot
   └─ Left Hand / Shield_slot
      └─ CorpseShieldHoldPoint

Enemy Skeleton
└─ Chest / UpperChest 계열
   └─ CorpseGripPoint
```

역할:

* `CorpseShieldHoldPoint`: Player 왼손의 실제 접촉 위치
* `CorpseGripPoint`: 시신의 견갑 / 흉갑 / 옷깃 쪽 실제 잡히는 위치
* `CorpseShieldPoseAnchor`: 시신 몸 전체의 방향과 사선 실루엣을 결정하는 회전 기준
* 시신을 부모 아래 붙일 때 부모 Lossy Scale을 역보정하여 원래 월드 크기를 유지

최종 확인된 PoseAnchor 회전 기준:

```text
Rotation X: -35
Rotation Y:  90
Rotation Z:  35
```

결과:

* 왼손이 시신의 상체를 잡음
* 시신 상반신이 Player 전면을 가림
* 골반과 다리가 Player 사선 아래로 내려감
* 발이 바닥 쪽으로 끌리는 실루엣이 읽힘
* 완전한 래그돌 / 발 IK는 사용하지 않음

### 2026-06-20 완료: 최소 야외 맵

사용 에셋:

```text
Low Poly Environment - Nature Free
LOWPOLY MEDIEVAL FANTASY SERIES
PolyTope Studio
```

이 에셋은 완성된 Terrain이나 Floor를 제공하는 맵 세트가 아니라 자연물 Prefab 중심의 팩이다.  
따라서 기존 Ground를 녹색 바닥으로 바꾸고, 자연물을 외곽에 배치하는 방식으로 최소 전투장을 구성했다.

맵 작업 원칙:

* Player / Enemy 이동과 AI 안정성을 위해 기존 평평한 Ground 유지
* 중앙에는 큰 나무 / 바위 / 관목을 두지 않음
* 자연물은 Cluster 하나를 만든 뒤 복제하여 네 방향 외곽에 배치
* Cluster마다 Rotation Y를 다르게 하여 같은 Prefab 반복을 숨김
* 하우징 / 레벨 디자인을 추가 스코프로 확대하지 않음

### Git 상태

* 에셋 작업 브랜치: `asset-integration`
* 2026-06-20: `asset-integration`을 `main`에 Merge
* 현재 Unity 완성본의 기준 브랜치는 `main`
* Merge 후 GitHub Desktop에 `Push origin`이 표시된다면 원격 `main` 반영을 위해 마지막 Push를 수행할 것
* `asset-integration` 브랜치는 당장 삭제하지 않고 에셋 통합 이력 / 백업으로 남겨도 됨

### 공식 종료 판단

현재 결과물은 상용 게임이나 완전판 Vertical Slice가 아니라 **첫 3D 액션 전투 기술 프로토타입**이다.  
무료 임시 모델과 애니메이션, 최소 맵을 사용하므로 시각적으로 조잡한 부분은 남아 있지만, 정한 기능과 에셋 통합 목표는 완료했다.

Unity에서 더 이상 필수로 구현할 기능은 없다. 이후 작업은 다음처럼 별도 단계로 취급한다.

* 포트폴리오용 영상 / README / 코드 설명
* 에셋 출처와 라이선스 정리
* 선택적 VFX / SFX / 조명 폴리시
* Unreal 비교 프로토타입
* 최종 유카 모델 / 정식 게임 제작

---

## 1. 에셋 적용 단계의 공식 목표

### 현재 공식 목표

**현재 에셋 통합 결과물 자체를 1차 취업 포트폴리오로 사용한다.**

별도의 “에셋 시험판”을 만든 뒤 다시 포트폴리오 버전을 제작하는 계획이 아니다.
현재 캡슐 전투 코드 위에 모델과 애니메이션을 하나씩 적용한 최종 상태가 곧 포트폴리오 결과물이다.

완료 조건은 아래 세 가지다.

1. 현재 Scene에 맵 / Player / Enemy 그래픽 에셋이 적용되어 캡슐 테스트장처럼 보이지 않는다.
2. Play 시 기존 전투 기능에 대응하는 Player / Enemy 애니메이션이 정상 재생된다.
3. 기존 전투 판정과 상태 구조가 에셋 적용 이후에도 정상 작동한다.

### 목표 범위

* Player 임시 인간형 모델 1개
* Enemy 인간형 남성 병사 모델 1종
* 동일 Enemy 모델 복제 사용 허용
* 작은 전투 맵 에셋 1개
* Player 이동 애니메이션
  * Idle
  * Walk 8방향
  * 일반 Sprint
  * 시신 방패 Run 8방향
* Player 행동 애니메이션
  * 3타 공격
  * 막기
  * 피격
  * 구르기 또는 스텝
  * 돌파
  * 시신 방패 상태를 읽을 수 있는 최소 자세
* Enemy 행동 애니메이션
  * Idle / 이동 / 선회 / 후퇴
  * 공격 1타 / 2타
  * Guard
  * Backstep
  * 피격
  * 현재 코드식 사망 / 시신 전환과 충돌하지 않는 표현
* 현재 HUD와 락온 표시는 기존 상태 유지 가능
* 실행 중 반복 Error 없이 전투 루프가 정상 작동할 것

### 현재 필수 범위에서 제외

* 사운드
* VFX
* 신규 UI 제작
* 짧은 스토리 진행 구조
* 플레이 영상 / README를 현재 에셋 통합 완료 조건으로 포함하는 것
* 최종 유카 캐릭터 모델 제작
* 최종 복장 / 얼굴 / 망토 디테일
* 커스텀 모션 전체 제작
* 고급 망토 Cloth 물리
* 래그돌
* 완전한 시체 물리
* 적 병종 다변화
* 대규모 스토리 챕터
* 마법 해제 / 무장해제 / 받아묶기 / 자동 패링
* 점프 기능
* 상용 게임 수준의 애니메이션 보정
* 완전판 Vertical Slice

### 완료 품질 기준

* T-Pose가 남지 않음
* 상태에 맞는 애니메이션이 재생됨
* 이동 방향과 애니메이션 방향이 심하게 충돌하지 않음
* 무기와 손 위치가 기능 확인을 방해할 정도로 이탈하지 않음
* 공격 / 방어 / 피격 / 회피 / 돌파 / 시신 방패 행동이 눈으로 구분됨
* 무료 에셋 조합 흔적은 허용
* 완벽한 유카 전용 자세보다 **깔끔하게 작동하는가**를 우선

현재 Player 모델과 애니메이션은 임시 검증용이다.
향후 한 챕터 규모의 게임이나 스팀 데모로 확장할 경우 모델 / 애니메이션 / 맵 구조를 크게 교체할 수 있다.

---


## 2. 비주얼 방향과 예산

### 비주얼 방향

1순위:

* 현실적인 다크 판타지
* 인물은 반드시 극사실적일 필요 없음
* 3D 형태 유지
* 전투 동작과 공격 방향이 명확하게 읽힐 것

대안:

* 현실적 다크 판타지 에셋 조합이 지나치게 어렵거나 비용이 높을 경우
* 아기자기하거나 귀여운 스타일라이즈드 3D로 전환 가능
* 단, 2D나 SD 전용 표현으로 바꾸지는 않음

### 품질 기준

* 무료 에셋 조합이라는 흔적이 보여도 허용
* 미술 통일성보다 전투 가독성이 우선
* 스크린샷 한 장의 화려함보다 실제 Play에서 상태와 방향이 읽히는지를 우선
* 모델의 외형보다 기존 전투 판정과의 안전한 결합을 우선
* 복장 / 얼굴 / 망토 디테일은 현재 범위에서 포기 가능
* 검을 쥔 한쪽 팔이 고정된 전용 달리기 모션까지 찾는 것은 현재 필수 요구가 아님

### 예산

* 1순위: 무료
* 유료 구매가 반드시 필요할 경우 전체 5만 원 이하
* 가능하면 0원 유지
* 현재 Player 모델과 애니메이션은 무료 에셋으로 진행
* 유료 구매 전 라이선스 / Unity 버전 / URP 호환 여부 확인
* 사운드와 VFX는 현재 목표에서 제외하므로 구매 대상에서도 제외

---


## 3. 절대 유지할 전투 구조

에셋은 기존 전투 구조에 맞춘다.  
에셋 때문에 작동 중인 판정과 상태 구조를 함부로 교체하지 않는다.

### Player 루트

`Player` 루트는 계속 다음 기능의 권위자다.

* Rigidbody 이동
* Capsule Collider
* PlayerController
* PlayerHealth
* BreakCharge
* CorpseShield
* LockOnSystem
* 공격 판정
* 구르기 무적
* 막기 판정
* 피격 / 넉백 / HitStun

모델과 Animator는 Player 루트 아래의 시각 계층에서 표현만 담당한다.

### PlayerAnimationBridge

`PlayerAnimationBridge.cs`는 기존 전투 코드를 대체하지 않는다.

현재 Animator에 전달하는 주요 값:

```text
MoveSpeed       Float
MoveX           Float
MoveY           Float
IsSprinting     Bool
IsCorpseRunning Bool
IsHoldingCorpse Bool
IsBlocking      Bool
Attack1         Trigger
Attack2         Trigger
Attack3         Trigger
BlockHit        Trigger
Hit             Trigger
Dodge           Trigger
Hit             Trigger
Dodge           Trigger
Hit             Trigger
Dodge           Trigger
```

`IsCharging`은 현재 `BreakCharge.cs`가 직접 Animator에 전달한다.

Bridge 역할:

* Player Rigidbody의 실제 수평 속도를 읽음
* Player 루트 기준 로컬 이동 방향을 계산
* 이동 파라미터를 Animator에 전달
* `PlayerController.IsSprinting` 전달
* `PlayerController.IsCorpseRunning` 전달
* `CorpseShield.IsHoldingCorpse` 전달
* `PlayerController.IsBlocking` 전달
* `AttackStartSequence` 변화 감지 후 현재 콤보 단계 Trigger 실행
* `PlayerHealth.BlockSuccessSequence` 변화 감지 후 `BlockHit` Trigger 실행
* `PlayerHealth.HitReactionSequence` 변화 감지 후 `Hit` Trigger 실행
* `PlayerController.DodgeStartSequence` 변화 감지 후 `Dodge` Trigger 실행

실제 이동 / 공격 / 방어 / 피격 판정 권한은 기존 전투 코드가 계속 보유한다.

### Root Motion

현재 적용에서는 **Root Motion을 사용하지 않는다.**

* `Apply Root Motion` 비활성화 유지
* 실제 이동은 기존 Rigidbody와 `PlayerController`가 담당
* 돌파 이동은 `BreakCharge.cs`가 담당
* 애니메이션은 이동을 시각적으로 표현
* 애니메이션 파일은 가능하면 In-Place 버전을 사용

### 일반 Walk / 락온 Walk

* 비락온 상태에서는 기존 코드가 이동 방향으로 Player 루트를 회전
* 락온 상태에서는 Player 루트가 Enemy를 바라봄
* `MoveX`, `MoveY`에 따라 전진 / 후진 / 좌우 / 대각선 Walk 재생

### 일반 Sprint

* 시신 방패가 없는 상태에서 Shift + 이동 입력으로 Sprint
* 락온은 유지 가능
* 몸은 실제 이동 방향으로 회전
* Shift 해제 후 락온 중이면 Enemy 방향으로 다시 회전

### 자유 방향 구르기

* 실제 방향 결정 / 이동 / 무적 판정은 `PlayerController`가 담당
* Animator는 `Dodge` Trigger와 `Roll Forward` 클립으로 표현만 담당
* 구르기 시작 시 현재 입력 방향을 `dodgeDirection`으로 저장
* 방향 입력이 있으면 Player 루트를 해당 방향으로 돌린 뒤 전방 구르기 재생
* 방향 입력이 없으면 현재 Player 정면으로 구름
* 락온 중에도 구르기 동안은 입력 방향을 정면으로 사용
* 구르기 종료 후 기존 락온 회전 규칙으로 Enemy를 다시 바라봄
* 전용 좌 / 우 / 후방 Roll은 현재 사용하지 않음
* 코드 기본값과 별개로 Unity Inspector에 저장된 `Dodge Duration` / 무적 시간 값을 반드시 확인

### 공격 3타

* 실제 콤보 단계와 입력 예약은 `PlayerController`가 담당
* `AttackStartSequence`는 공격 시작마다 증가
* `CurrentComboStep`은 현재 1 / 2 / 3타를 외부에 제공
* Animator는 `Attack1 / Attack2 / Attack3` Trigger로 시각 표현만 담당
* 현재 공격 구성은 약공 → 약공 → 강공
* 공격 판정은 `SwordHitBox` Transform의 월드 위치 / 회전과 `weaponHitBoxHalfExtents`를 사용
* `SwordHitBox` Transform Scale은 실제 판정 크기를 바꾸지 않음
* 활성 시간 동안 적을 찾을 때까지 매 프레임 검사
* 실제 적중 후에만 해당 타격의 추가 검사를 중단
* Animation Event로 판정 권한을 옮기지 않음

현재 기준:

```text
1타 전체 1.10 / 판정 0.30 ~ 0.67
2타 전체 1.33 / 판정 0.77 ~ 1.03
3타 전체 1.33 / 판정 0.53 ~ 0.87
```

### 일반 방어

* 실제 방어 가능 여부와 데미지 무시는 `PlayerController` / `PlayerHealth`가 담당
* `BlockLocomotion`은 방어 정지와 4방향 이동을 표현
* `BlockHit`은 실제 방어 성공 때만 재생
* `BlockSuccessSequence`가 증가할 때 `PlayerAnimationBridge`가 Trigger 실행
* 일반 방어 중에는 `TaoMu Sword_Block`을 표시
* Sprint 중 우클릭은 돌파가 우선이므로 Sprint에서 Block 상태로 직접 전환하지 않음

### 돌파

* `BreakCharge.cs`가 입력 / 이동 / 판정 / 리커버리를 담당
* Animator가 돌파 이동을 대신하지 않음
* Animator Bool: `IsCharging`
* 돌파 중 `TaoMu Sword_Charge`를 표시
* 일반 검과 방어용 검은 숨김
* 돌파 종료 후 현재 입력 상태에 맞는 검 외형으로 복귀
* WASD 입력이 있으면 카메라 기준 방향으로 돌파
* 무입력 시 카메라 정면
* 락온 중에도 W/S/A/D 방향 기동 유지
* Guard 파괴 구조 유지

### 시신 방패

입력은 E 토글 방식이다.

* E 1회: 가까운 시신 집기
* E 재입력: 현재 시신 폐기
* 시신 보유 중 구르기 / 일반 막기 / 돌파 제한 유지
* Enemy 공격 1회 차단 후 시신 소모
* 좌클릭 공격 시 시신 소모 후 일반 공격으로 전환

애니메이션 상태:

* `IsHoldingCorpse = true`
  * `CorpseShieldLocomotion`
* `IsCorpseRunning = true`
  * `CorpseRun`

`CorpseShieldHoldPoint`는 왼손 `Shield_slot` 아래의 실제 접촉점으로 둔다.  
시신의 `CorpseGripPoint`를 이 위치에 맞추고, 시신 몸 전체 방향은 Player 루트 아래 `CorpseShieldPoseAnchor`가 담당한다. 최종 PoseAnchor 회전 기준은 `(-35, 90, 35)`다.

### 대검 외형 교체

현재 대검 외형은 세 개를 사용한다.

```text
TaoMu Sword         일반 공격 / 일반 이동
TaoMu Sword_Block   일반 방어
TaoMu Sword_Charge  돌파
```

세 검은 같은 오른손 무기 슬롯 아래에 두고, 현재 상태에 맞는 하나만 표시한다.  
`SwordHitBox`는 별도 형제 오브젝트로 유지하며 일반 공격 판정에만 사용한다.

### Enemy

* Enemy 루트의 Rigidbody / Capsule Collider / AI / 공격 판정 유지
* Enemy 모델은 시각 자식으로 배치
* EnemyAttack의 공격 판정은 기존 Box 판정 유지
* EnemySimpleAI의 상태 판단은 코드가 담당
* Animator는 Patrol / Approach / Guard / Backstep / Attack 등을 표현
* DoubleL의 `RPG_Animations_Pack_FREE`에 포함된 무장 인간형 모델과 전투 클립을 Enemy 후보로 우선 검토

### 사망과 시신 전환

초기에는 현재 코드식 사망을 유지한다.

1. Enemy AI / 공격 정지
2. Rigidbody 정지
3. 루트가 천천히 옆으로 기울어짐
4. 일정 시간 바닥에 남음
5. 동일 오브젝트가 `CorpseObject`로 전환
6. 시신 방패로 사용 가능

초기에는 사망 애니메이션 / 래그돌을 이 구조보다 우선하지 않는다.

---

## 4. 현재 Player 비주얼 / Animator 구조

### 현재 Hierarchy 핵심 구조

```text
Player
├─ 기존 전투 컴포넌트
├─ PlayerVisualRoot
│  └─ PT_Boy_Modular_Free_Pack
│     ├─ Skinned Mesh 파츠
│     ├─ Humanoid Bones
│     ├─ Animator
│     ├─ PlayerAnimationBridge
│     ├─ Right Hand Weapon Slot
│     │  ├─ SwordHitBox
│     │  ├─ TaoMu Sword
│     │  ├─ TaoMu Sword_Block
│     │  └─ TaoMu Sword_Charge
│     └─ Left Hand Shield Slot
│        └─ CorpseShieldHoldPoint
└─ 기존 Collider / Rigidbody / 전투 판정
```

실제 뼈 / 슬롯 이름은 에셋의 Hierarchy 표시명을 기준으로 한다.

### 역할 분리

`Player`

* 실제 이동
* 회전
* 충돌
* 전투 상태
* 데미지 판정

`PlayerVisualRoot`

* 모델 전체 위치 / 크기 / 전방 보정
* 시각 계층 정리

`PT_Boy_Modular_Free_Pack`

* Skinned Mesh
* Humanoid Rig
* Avatar
* Animator
* PlayerAnimationBridge
* 무기 슬롯과 손 뼈

### 현재 Animator 설정

```text
Controller: PlayerAnimator
Avatar: PT_Boy_Modular_Free_PackAvatar
Apply Root Motion: Off
```

### 현재 Animator 파라미터

```text
MoveSpeed       Float
MoveX           Float
MoveY           Float
IsSprinting     Bool
IsCorpseRunning Bool
IsHoldingCorpse Bool
IsCharging      Bool
IsBlocking      Bool
Attack1         Trigger
Attack2         Trigger
Attack3         Trigger
BlockHit        Trigger
```

### 현재 Animator 상태

```text
Entry → Idle
Idle ↔ Locomotion
Idle / Locomotion ↔ Sprint
Idle / Locomotion → Attack1 / Attack2 / Attack3
Idle / Locomotion → BreakCharge
Idle / Locomotion → BlockLocomotion
BlockLocomotion → BlockHit
BlockHit → BlockLocomotion 또는 Idle
Idle / Locomotion → CorpseShieldLocomotion
CorpseShieldLocomotion ↔ CorpseRun
Any State → Hit
Hit → Idle
Any State → Dodge
Dodge → Idle
```

공격 상태는 Trigger와 현재 코드의 콤보 시작 신호로 진입한다.  
지속 상태인 방어 / 시신 방패 / 돌파는 Bool 조건을 사용한다.

### Locomotion

* `2D Simple Directional`
* Walk 8방향
* X: `MoveX`
* Y: `MoveY`

### Sprint

* 일반 상태 전진 Sprint 단일 클립
* `IsSprinting = true`
* 몸은 실제 이동 방향을 향함

### BlockLocomotion

* `2D Simple Directional`
* 중앙: `BlockIdle` Mirror On
* 전진 / 후진 / 좌 / 우: BlockWalk 4방향 Mirror Off
* 대각선은 자동 혼합
* `IsBlocking = true`

### BlockHit

* Loop Off
* Mirror On
* 실제 방어 성공 Trigger에서만 재생
* 우클릭 유지 중이면 BlockLocomotion 복귀
* 우클릭 해제 상태면 Idle 복귀

### CorpseShieldLocomotion

* `2D Simple Directional`
* 중앙: 원본 BlockIdle Mirror Off
* 전진 / 후진 / 좌 / 우: BlockWalk 4방향 Mirror On
* Mirror 때문에 좌우 이동 클립을 서로 바꿔 배치
* `IsHoldingCorpse = true`

### CorpseRun

* `2D Simple Directional`
* 물체를 든 Run 4방향을 Mirror하여 왼손 보유 자세로 사용
* 대각선은 자동 혼합
* `IsCorpseRunning = true`

### 공격 상태

* `Attack1`: 약공
* `Attack2`: 약공
* `Attack3`: 강공
* Loop Off
* 공격 클립 일부는 Start / End 프레임을 잘라 사용
* 후반 이음새가 약간 끊기는 것은 허용

### Hit

* Trigger: `Hit`
* Loop Off
* 실제 HP 감소 시에만 재생
* `Any State → Hit`
* 공격 / 이동 중 실제 피격 시 현재 행동을 끊고 피격 반응
* 종료 후 Idle 복귀

### Dodge

* Trigger: `Dodge`
* Motion: `Roll Forward`
* Loop Off
* Start 2 / End 35
* `Any State → Dodge`
* `Dodge → Idle`
* Exit Time은 구르기 동작이 완전히 끝난 뒤 복귀하도록 설정
* 방향별 전용 Blend Tree는 사용하지 않음

### 대검 표시 규칙

| 상태 | 표시 검 |
|---|---|
| 일반 이동 / 공격 | `TaoMu Sword` |
| 일반 방어 | `TaoMu Sword_Block` |
| 돌파 | `TaoMu Sword_Charge` |

### 모델 정렬 규칙

* Player 루트 Transform은 가급적 건드리지 않음
* 모델 크기 / 위치 / 방향은 `PlayerVisualRoot`에서 조정
* Scale X / Y / Z는 동일한 값 사용
* Capsule Collider는 삭제하지 않음
* 기존 캡슐 Mesh Renderer만 비활성화 가능
* 무기 / Hold Point 위치는 각 손 슬롯 아래에서 조정
* Play 중 수정한 Transform 값은 종료 시 되돌아가므로 필요하면 Copy / Paste Component Values 사용

### Nose 오브젝트

`nose`는 캡슐의 앞뒤를 확인하기 위한 임시 시각 표시물이었다.

* 기능 스크립트 없음
* 현재 인간형 모델로 방향 확인 가능
* 코드에서 참조하지 않음
* 삭제 가능

---

## 5. Player / Enemy 에셋 요구 조건

### Player 모델

필수:

* 남성
* 성인
* 건장하지만 과도한 근육질은 아님
* 소년을 연상시키는 젊은 인상
* 얼굴 노출
* 갑옷 비중이 낮음
* 대검 뒤로 상당 부분 숨을 수 있는 체형
* 3D
* Humanoid Rig
* URP 호환 또는 URP 변환 가능
* 손 / 팔 / 척추 / 머리 본이 정상
* 오른손 대검 장착 가능
* 왼손 시신 방패 자세 보정 가능

망토:

* 유카에게 망토가 있다는 설정은 유지
* 초기에는 망토가 없어도 허용
* 망토가 있더라도 Cloth 물리는 보류
* 고정 망토 또는 뼈 기반 단순 움직임 우선
* 구르기 / 대검 / 시신 방패 관통 문제를 감수하며 처음부터 Cloth를 넣지 않음

### Player 전투 자세

유카의 대검 운용은 단일 표준 모션으로 해결되지 않는다.

* 평상시 양손 사용 가능
* 일부 행동은 한손 대검
* 돌파는 한 손이 손잡이, 다른 손이 검등을 받치는 자세
* 시신 방패는 왼손으로 적의 견갑 / 옷깃 / 멱살 부근을 잡아 세움
* 오른손에는 대검 유지
* 공격 입력 시 시신을 버리고 대검 공격으로 전환

초기 적용 원칙:

* 완벽한 정식 유카 모션을 만들지 않음
* 가장 가까운 무료 애니메이션으로 행동을 먼저 읽히게 함
* 돌파 / 시신 방패 등 특수 자세만 나중에 별도 보정
* 필요 시 Animation Rigging / IK / 클립 편집을 검토하되 초기 범위에서는 보류

### Enemy 모델

첫 Enemy는 다음으로 고정한다.

* 인간형 남성 병사
* 한손검 우선
* 방패 없음
* 경갑 또는 중갑
* Guard 가능
* 짧은 백스텝 가능
* 얼굴 노출 여부는 중요하지 않음
* Humanoid Rig
* Player와 스타일이 심하게 충돌하지 않을 것
* 모델 1종만 사용
* 같은 모델을 복제하여 다수전 테스트 가능

첫 Enemy를 한손검 병사로 두는 이유:

* 현재 EnemyAttack의 짧은 공격 거리와 Box 판정에 맞추기 쉬움
* Guard를 검 방어로 표현 가능
* 창병보다 사거리 재조정이 적음
* Player 대검과 시각적 역할이 덜 겹침


---

---

## 6. 필요한 애니메이션 목록과 현재 우선순위

### Player 완료

1. Idle ✅
2. Walk 8방향 ✅
3. 일반 Sprint ✅
4. 오른손 대검 부착 ✅
5. 대검 1타 ✅
6. 대검 2타 ✅
7. 대검 3타 ✅
8. BreakCharge ✅
9. Block 정지 ✅
10. Block 4방향 이동 ✅
11. BlockHit ✅
12. Corpse Shield 정지 ✅
13. Corpse Shield 일반 이동 ✅
14. Corpse Shield Run ✅
15. 왼손 Hold Point 생성 / 연결 ✅
16. 일반 피격 `Hit Reaction` ✅
17. 자유 방향 구르기 `Dodge` ✅

### Player 후속 보정

Player 필수 애니메이션은 완료했다.

1. Enemy 인간형 모델 적용 후 시신 방패 위치 / 회전 최종 조정
2. Enemy 공격 애니메이션 적용 후 구르기 무적 구간 미세 조정
3. 전체 행동 회귀 테스트

전용 좌 / 우 / 후방 구르기는 현재 범위에서 구현하지 않는다.  
입력 방향으로 Player 루트를 회전한 뒤 `Roll Forward`를 재사용하는 방식으로 자유 방향 구르기를 완성했다.

### Enemy 남은 필수

1. 인간형 남성 병사 모델 1종
2. 한손검 외형 확인
3. 전투 Idle
4. Patrol / Walk
5. Approach / Run
6. 좌측 선회
7. 우측 선회
8. 후퇴
9. Backstep
10. Guard
11. Attack 1
12. Attack 2
13. Hit Reaction
14. 현재 코드식 사망 / 시신 전환과 충돌하지 않는 기본 자세

코드 상태마다 전용 클립을 하나씩 만들 필요는 없다.

```text
AlertStare / CloseFaceOff → 전투 Idle 재사용
Patrol → Walk
Approach / ApproachAttack / Return → Walk 또는 Run 재사용
AlertCircle / CloseCircle → Strafe Left / Right
Flee → Backward 또는 Run 재사용
```

`RPG_Animations_Pack_FREE`와 `FREE 32 RPG Animations`에 Enemy에 재사용할 수 있는 이동 / 공격 / 방어 / 피격 후보가 있으므로 우선 검토한다.

### 맵 남은 필수

* 현재 회색 Ground와 테스트 공간을 대체할 작은 전투 맵 1개
* 넓은 월드 제작 금지
* 카메라에 보이는 범위와 전투 가독성 우선
* 접근 불가능한 내부 공간을 불필요하게 제작하지 않음

### 현재 적용 순서

1. Enemy 인간형 모델 적용
2. Enemy 전투 Idle / 이동 / 응시 / 선회
3. Enemy 공격 1 / 2
4. Enemy Guard / Backstep / Hit
5. Enemy 사망 / 시신 전환 확인
6. 시신 방패 실제 인간형 위치 최종 조정
7. 구르기 무적 시간 최종 조정
8. 작은 맵 에셋 배치
9. 전체 회귀 테스트

### 애니메이션 선택 규칙

* Root Motion 중심 구조로 바꾸지 않음
* 가능하면 In-Place 버전 사용
* 상태가 읽히면 무료 에셋의 손가락 / 손목 오차 허용
* 공격 / 돌파 / 방어 후반 이음새가 약간 끊겨도 기능 가독성이 확보되면 통과
* Jump 클립이 있어도 점프 기능을 구현하지 않음
* Player 자유 방향 구르기는 `Roll Forward` 하나를 방향 회전과 결합해 사용
* 이름이 다른 방향 Roll이라도 실제 프리뷰 결과가 전방 구르기와 차이가 작으면 억지로 연결하지 않음

---

## 7. 현재 디버깅 이력

### Player 모델 분홍색 표시

증상:

* 모델 전체가 분홍색으로 표시

원인:

* Built-in 또는 기존 Shader 머티리얼이 URP 프로젝트와 맞지 않음

해결:

* 공용 머티리얼 `modular_NPC` 선택
* Shader를 `Universal Render Pipeline/Lit`으로 변경
* Base Map에 기존 색상 텍스처 연결

특징:

* 머리 파츠의 머티리얼을 변경했는데 전신이 함께 바뀜
* 여러 Skinned Mesh 파츠가 같은 `modular_NPC` 머티리얼을 공유하고 있었기 때문
* 공용 머티리얼 구조는 정상

주의:

* 프로젝트 전체 머티리얼 일괄 변환 금지
* 필요한 에셋의 머티리얼만 변환
* 기존 대검 / Ground / 기타 정상 머티리얼을 불필요하게 건드리지 않음

### 모델 원본 FBX / Rig 위치 찾기 어려움

해결 방식:

1. Hierarchy의 실제 모델 파츠 선택
2. `Skinned Mesh Renderer` 확인
3. Mesh 항목의 파일 이름 클릭
4. Project 창에서 원본 모델 파일 역추적
5. Inspector의 `Rig` 탭 확인
6. Animation Type `Humanoid`
7. Configure에서 초록색 뼈 확인

### PlayerVisualRoot 계층 이동 문제

증상:

* 기존 빈 `PlayerVisualRoot` 아래로 Prefab 모델을 드래그하기 어려움

해결:

* 모델 루트 우클릭
* `Create Empty Parent`
* 새 부모 이름을 `PlayerVisualRoot`로 변경

결과:

```text
Player
└─ PlayerVisualRoot
   └─ PT_Boy_Modular_Free_Pack
```

### Inspector MissingReferenceException

메시지:

```text
MissingReferenceException:
The variable m_Targets of GameObjectInspector doesn't exist anymore.

SerializedObjectNotCreatableException:
Object at index 0 is null
```

판단:

* Hierarchy 계층 변경 중 Inspector가 사라진 오브젝트를 잠시 참조한 Unity Editor 예외
* 전투 코드 오류 정황 없음

처리:

1. Play 종료
2. 존재하는 다른 오브젝트 선택
3. Inspector Lock 해제 확인
4. Console Clear
5. Scene 저장
6. 필요 시 Unity 재실행

매 프레임 반복되지 않으면 무시 가능.

### T-Pose

원인:

* Avatar는 있으나 Animator Controller가 None
* 재생할 상태가 없었음

해결:

* `PlayerAnimator` 생성
* 제자리 Idle 클립을 기본 상태로 등록
* 모델 Animator의 Controller에 연결
* Root Motion 비활성화

결과:

* 정상 Idle 자세 전환
* Play 중 문제없이 반복 재생

### Git 변경 파일 1262개

판단:

* 캐릭터 모델 팩과 애니메이션 팩 Import로 실제 Assets 파일과 `.meta` 파일이 대량 추가된 결과
* `.gitignore`가 있어도 Assets는 프로젝트의 실제 재료이므로 Git에 포함되는 것이 정상

정상 경로 예:

```text
Assets/...
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/...
```

비정상 경로 예:

```text
Library/...
Temp/...
Logs/...
Obj/...
UserSettings/...
```

주의:

* 파일 수 자체보다 경로가 중요
* 대부분 `Assets`와 `.meta`면 정상
* `Library` 등이 잡히면 커밋 중단 후 `.gitignore` 확인

---

### 이동 방향이 전부 전진 Walk로 보임

초기 증상:

* `MoveSpeed` 하나만 Animator에 전달
* 락온 중 후진 / 좌우 이동도 전진 Walk로 재생

원인:

* Animator가 이동 여부만 알고 로컬 이동 방향을 알지 못했음

해결:

* `MoveX`, `MoveY` Float 파라미터 추가
* Rigidbody 수평 속도를 Player 루트 기준 로컬 방향으로 변환
* Walk 8방향을 `2D Simple Directional` Blend Tree에 배치

결과:

* 락온 중 전진 / 후진 / 좌우 / 대각선 정상 재생
* 비락온 상태는 기존 회전 코드에 따라 이동 방향을 바라보며 전진 Walk 표현

### Sprint에 후진 클립이 없음

상황:

* Run은 8방향 세트 존재
* Sprint는 Backward / BackwardLeft / BackwardRight가 없음

판단:

* 뒷걸음질 전력질주는 동작상 부자연스러움
* Sprint 시 몸 자체를 실제 이동 방향으로 돌리는 것이 자연스러움

해결:

* 일반 상태 Shift 이동은 락온 여부와 무관하게 Sprint 사용
* 락온은 유지하되 Player 몸은 실제 이동 방향으로 회전
* Shift 해제 후 기존 Enemy 응시 방향으로 복귀

### 시신 방패 오른쪽 대각선 입력 불가

증상:

* E 홀드 상태에서 `W + D`, `S + D` 입력이 되지 않음
* `W + A`, `S + A`는 정상
* Walk / Run 모두 실제 이동 자체가 발생하지 않음

원인:

* 코드나 Animator가 아니라 키보드의 특정 3키 동시 입력 고스팅 / 롤오버 제한

해결:

* 시신 방패 입력을 E 홀드에서 E 토글로 변경
* E 1회로 집기, E 재입력으로 폐기

결과:

* 시신 보유 상태에서 오른쪽 / 왼쪽 대각선 Walk와 Run 모두 정상

주의:

* New Input System으로 변경해도 하드웨어가 키 입력을 보내지 않으면 해결되지 않음
* E 홀드 방식으로 되돌리지 말 것

### 돌파 중 Walk 애니메이션 재생

현재 증상:

* 돌파 기능과 판정은 정상
* 돌파 중 시각 모델은 Walk 계열 애니메이션을 재생

원인:

* BreakCharge가 Rigidbody를 이동시키므로 `MoveSpeed > 0`
* 돌파 전용 Animator 파라미터 / 상태가 아직 없음
* `IsSprinting`은 외부 액션 잠금 중 false

판단:

* 현재는 돌파 애니메이션 미구현 상태이므로 정상
* 돌파 기능 오류가 아님

추후 해결:

* BreakCharge의 현재 상태를 읽는 별도 Animator Bool 또는 Trigger 추가
* 돌파 전용 클립 연결
* 실제 이동 권한은 계속 BreakCharge가 유지

---


### 2026-06-17 SwordHitBox가 Ground에 박히거나 Scale이 안 먹음

증상:

* 검을 오른손 슬롯에 붙인 뒤 판정 박스가 Ground 쪽에 남거나 지나치게 큼
* `SwordHitBox` Transform Scale을 바꿔도 Gizmo 크기가 변하지 않음

원인:

* 판정 위치 / 회전은 `SwordHitBox` Transform을 사용
* 판정 크기는 `PlayerController.weaponHitBoxHalfExtents`를 사용
* Transform Scale은 판정 크기에 반영되지 않음

해결:

* `SwordHitBox`를 오른손 슬롯 아래에서 검과 형제로 유지
* Position / Rotation만 검날에 맞춤
* 실제 크기는 `weaponHitBoxHalfExtents`에서 조정

### 2026-06-17 공격 애니메이션과 코드 시간이 서로 다름

증상:

* 공격 애니메이션 도중 이동이 먼저 풀려 미끄러짐
* 검을 뒤로 빼는 순간 판정 활성
* 1타가 끝나기 전에 2타 / 3타가 덮어씀

원인:

* 기존 코드 공격 시간은 캡슐 기준
* 새 애니메이션의 실제 준비 / 타격 / 회수 시간이 더 김

해결:

* 클립 Start / End 프레임 절삭
* `GetAttackDuration()` 조정
* `GetAttackHitStartTime()` 조정
* `GetAttackHitEndTime()` 조정
* 한 타씩 단독 테스트 후 3타 연결

### 2026-06-17 활성 히트박스 안의 Enemy가 맞지 않음

증상:

* Gizmo는 붉게 활성화
* 실제로 Enemy와 겹쳐도 데미지 없음

원인:

* 활성 구간 첫 프레임에 `hasHitDuringThisAttack = true`가 되어 이후 프레임 검사를 중단

해결:

* 활성 구간 동안 실제 Enemy를 찾을 때까지 계속 검사
* 실제 적중 순간에만 `hasHitDuringThisAttack = true`

### 2026-06-17 BreakCharge가 스케이팅처럼 보임

증상:

* 실제 Player 루트는 전진하지만 모델은 첫 자세에 고정

원인 / 해결:

* `Any State → BreakCharge`의 `Can Transition To Self`를 꺼 자기 상태 재진입 방지
* 반복 이동 클립은 `Loop Time` 활성화

### 2026-06-17 BlockHit이 보이지 않음

증상:

* 카메라만 흔들리고 신체 반응 없음

원인:

* `Any State → BlockIdle`이 `IsBlocking = true`인 동안 `BlockHit`을 즉시 덮어씀

해결:

* Any State 방어 진입 제거
* Idle / Locomotion에서만 BlockLocomotion 진입
* BlockLocomotion → BlockHit 직접 연결

### 2026-06-17 방어 카메라 흔들림이 신체 반응보다 빠름

해결:

* BlockHit 시작 신호 먼저 발생
* 카메라 흔들림을 약 0.03초 지연

### 2026-06-17 CorpseShield 전환 화살표 조건 혼동

증상:

* 걷기와 달리기가 구분되지 않음
* 정지 중에도 제자리걸음

원인:

* `IsHoldingCorpse`와 `IsCorpseRunning` 조건을 잘못 연결
* 과거 CorpseRun 직접 전환 화살표 일부가 남아 있음

해결:

* `CorpseShieldLocomotion ↔ CorpseRun`만 직접 연결
* Bool 조건을 정확히 분리
* 오래된 Idle / Locomotion / Sprint ↔ CorpseRun 화살표 삭제

### 2026-06-17 Player HP Bar가 잘려 보임

원인:

* Game View에서 마우스 휠로 화면이 확대됨
* Canvas / PlayerHUD 문제 아님

해결:

* Game View 줌을 원래 배율로 복원

### 2026-06-17 임시 시신 캡슐이 Player를 가림

판단:

* Hold Point와 왼손 추적은 정상
* 현재 Enemy 캡슐 Mesh가 인간형 시신보다 지나치게 큼
* Enemy 루트 Scale / Collider를 줄여 해결하지 말 것

추후:

* Enemy 인간형 모델 적용
* 임시 캡슐 Mesh Renderer 숨김
* `corpseLocalPosition` / `corpseLocalEulerAngles` 최종 조정



### 2026-06-18 일반 피격 애니메이션 연결

결과:

* 실제 HP 감소 시에만 `Hit` 실행
* 일반 방어 성공은 `BlockHit`
* 시신 방패 차단 / 구르기 무적 성공은 `Hit` 미실행
* 기존 넉백 / HitStun / 카메라 흔들림과 피격 모션 정상 결합

### 2026-06-18 구르기 애니메이션 도중 방향이 락온 대상으로 끌려감

증상:

* 락온 중 입력 방향으로 구르기 시작
* 몸이 땅에 닿고 일어나는 후반에 Enemy 방향으로 끌려가듯 회전
* 구르기 애니메이션 중인데도 Enemy 공격에 맞음

실제 원인:

* `PlayerController`의 public 기본값은 수정했지만, 이미 Scene의 Player 컴포넌트 Inspector에 과거 값이 저장되어 있었음
* 소스 기본값 변경은 기존 컴포넌트의 직렬화된 Inspector 값을 자동으로 덮어쓰지 않음
* 실제 실행에서는 과거 `dodgeDuration` / 무적 시간이 먼저 끝나고, 애니메이션만 계속 재생되고 있었음

해결:

* Player Inspector에서 직접 다음 값을 입력
  * Dodge Duration: `1.10`
  * Dodge Invincible Start Time: `0.10`
  * Dodge Invincible End Time: `1.03`
* 구르기 코드 상태와 애니메이션 종료 시간을 일치시킴
* 원인 추적 중 추가했던 회전 잠금 / 락온 차단 / 이동 차단 우회 코드는 제거
* 현재는 입력 방향으로 끝까지 구르고, 완료 후에만 락온 Enemy를 다시 바라봄

주의:

* public 필드 기본값을 소스에서 바꾼 뒤에는 Inspector 실제 값도 반드시 확인
* 문제 재현 시 락온 스크립트를 먼저 의심하기 전에 현재 실행 중인 Inspector 수치를 확인

### 2026-06-18 4방향 구르기 미사용 결정

확인한 에셋:

* `Roll Forward`
* `Roll Left`
* `Roll Right`
* `Roll Backward`

판단:

* 방향별 클립이 락온 방향을 유지하는 전용 Side Roll이라기보다, 해당 방향을 정면으로 삼는 전방 구르기에 가까움
* 현재 코드가 이미 입력 방향으로 Player를 회전시킨 뒤 `Roll Forward`를 재생
* 네 클립을 모두 연결해도 화면 결과 차이가 작고 Animator / 코드만 복잡해질 가능성이 큼

결론:

* Player는 `Roll Forward` 하나로 자유 방향 구르기 구현
* 락온 중에도 입력 방향으로 굴러간 뒤 Idle 복귀 시 Enemy를 다시 바라봄
* 전용 4방향 Dodge Blend Tree는 만들지 않음


### 2026-06-20 시신을 들면 크기가 비대해짐

* 원인: 시신 루트를 왼손 뼈 아래로 Parenting하면서 손 뼈 계층의 Scale을 상속
* 해결: 시신의 원래 World Scale을 저장하고 새 부모의 Lossy Scale로 나눠 Local Scale 보정
* 결과: 살아 있을 때와 들었을 때의 시신 크기 일치

### 2026-06-20 시신의 가랑이 / 등판을 잡는 것처럼 보임

* 원인: Enemy 루트 피벗을 HoldPoint에 직접 맞추면 골반이 접촉점이 됨
* 해결: Enemy Chest / UpperChest 계열 아래 `CorpseGripPoint` 생성
* `CorpseGripPoint`를 Player의 `CorpseShieldHoldPoint`에 일치시킴
* 잡는 위치와 몸 전체 회전을 분리하기 위해 `CorpseShieldPoseAnchor` 추가

### 2026-06-20 시신 사선 회전 축 혼동

* Enemy 방향을 Y 90도로 돌린 상태에서는 PoseAnchor Z가 기대한 상하 사선이 아니라 몸통 Roll처럼 보일 수 있음
* 실제 머리-발 방향의 사선은 PoseAnchor X를 조절하여 맞춤
* 최종 확인값: Rotation `X -35 / Y 90 / Z 35`

### 2026-06-20 환경 에셋에 완성 Floor가 없음

* `Low Poly Environment - Nature Free`는 완성 Terrain / Floor가 아니라 자연물 Prefab 중심 팩
* Project 창의 `Flowers` 폴더를 `Floors`로 오인하지 말 것
* 기존 Ground에 녹색 머티리얼을 적용하고 자연물은 외곽 장식으로 사용
* 나무 / 바위 / 관목 / 풀을 Cluster로 묶고 Cluster 전체를 복제
* Rotation Y와 균일 Scale을 바꿔 반복감을 줄임
* 중앙 전투 공간은 비워둠

### 2026-06-20 Unity API deprecation 경고

* `FindFirstObjectByType` 등 구식 API 관련 경고는 최신 검색 API로 교체 가능
* Legacy Input Manager 경고는 현재 입력 구조 때문에 의도적으로 허용
* Active Input Handling을 Input System 전용으로 바꾸지 말 것

## 8. 현재 상태와 다음 작업

### 현재 상태: Unity 스코프 완료

2026-06-20 기준 현재 Scene은 다음 전투 흐름을 실제 인간형 모델과 애니메이션으로 수행한다.

```text
Enemy 순찰
→ Player 발견
→ 응시 / 접근 / 선회
→ 공격 / Guard / Guarded CloseCircle
→ Player 공격 / 방어 / 구르기 / 돌파
→ Enemy Hit / HeavyHit / Death
→ CorpseObject 전환
→ 시신 방패 집기 / 이동 / 1회 방어 / 공격 전환 소모
```

Scene은 녹색 Ground와 자연물 Cluster가 배치된 소형 야외 전투장으로 변경됐다.  
따라서 프로젝트는 더 이상 캡슐 기능 시험장이나 회색 그레이박스 단계가 아니다.

### 완료 판정

#### 프로젝트 / 공통

* URP ✅
* Root Motion Off ✅
* 인간형 Player / Enemy ✅
* HP UI ✅
* Screen Space 락온 마커 ✅
* 인간형 1:1 전투 루프 ✅
* 최소 야외 맵 ✅
* 반복 Error 없음 ✅
* Kinematic 속도 Warning 제거 ✅
* 에셋 브랜치 `main` Merge ✅

#### Player

* 이동 / Sprint / 락온 이동 ✅
* 공격 3타 ✅
* Dodge / 무적 ✅
* Block / BlockHit ✅
* BreakCharge ✅
* 일반 Hit ✅
* CorpseShieldLocomotion / CorpseRun ✅
* 인간형 시신 방패 최종 정렬 ✅

#### Enemy

* 검방 인간형 모델 ✅
* 이동 / 선회 / 후퇴 ✅
* 공격 1타 / 2타 ✅
* Guard / Guarded CloseCircle ✅
* Hit / HeavyHit / Death ✅
* 사망 후 CorpseObject 전환 ✅

### 현재 허용하는 어색함

다음은 미구현이 아니라 현재 범위에서 허용한 폴리시 부족이다.

* 무료 클립 사이 전환이 완전히 매끄럽지 않음
* Enemy Guarded CloseCircle에서 방패가 크게 들썩임
* 공격 클립 절삭 구간의 이음새가 일부 끊김
* 손가락 / 손목이 무기나 시신을 완벽히 잡지 못함
* 돌파 자세가 정식 유카 전용 모션이 아님
* 시신의 관절이 완전히 축 늘어지는 래그돌은 아님
* 야외 맵은 자연물 Cluster를 이용한 최소 배치이며 전문 레벨 디자인이 아님
* VFX / SFX / 고급 조명 폴리시 없음

이 항목들은 현재 Unity 종료 판정을 뒤집지 않는다.

### 이후 선택 작업

Unity 기능 개발과 에셋 통합은 종료한다. 다음은 별도 작업으로만 진행한다.

1. GitHub 원격 `main` Push 최종 확인
2. 포트폴리오용 40~60초 플레이 영상
3. README와 구현 시스템 설명
4. 에셋 출처 / 라이선스 기록 보완
5. 필요할 때만 최소 VFX / SFX / 조명 보정
6. Unreal 비교 프로젝트 시작

Unity 프로젝트를 다시 열 때는 새 기능을 무작정 추가하지 말고, 명확한 포트폴리오 목적이나 회귀 버그가 있을 때만 수정한다.

---

## 9. Git 브랜치 운용

### 현재 브랜치 상태

`main`

* 캡슐 전투 코드와 에셋 통합 완성본이 합쳐진 현재 기준 브랜치
* 2026-06-20 이후 Unity 완성본의 공식 기준점

`asset-integration`

* Player / Enemy / 맵 그래픽과 애니메이션 통합 작업 이력
* 2026-06-20 `main`에 Merge 완료
* 즉시 삭제할 필요 없음
* 에셋 통합 직전 / 과정 이력을 확인하는 백업 브랜치로 보존 가능

### Merge 결과

다음 조건을 충족한 상태에서 `asset-integration → main` Merge를 수행했다.

* Player 그래픽 / 필수 애니메이션 완료
* Enemy 그래픽 / 필수 애니메이션 완료
* Screen Space 락온 완료
* 인간형 시신 방패 정렬 완료
* 작은 야외 맵 완료
* 기존 이동 / 공격 / 피격 / 돌파 / Guard / 시신 방패 유지
* 반복 Error 없음
* Scene 저장 및 Play 확인

### Merge 후 확인

GitHub Desktop에서 Current Branch가 `main`인지 확인한다.

```text
Current Branch: main
```

Merge 직후 `Push origin`이 나타나면 눌러 원격 저장소의 `main`에 반영한다.  
`Fetch origin` 후 `Pull origin`이 나타나지 않는 것은 원격에서 가져올 새 커밋이 없다는 뜻이며 오류가 아니다.

### 대량 에셋 파일

* 캐릭터 / 애니메이션 / 환경 에셋 Import로 1000개 이상의 변경 파일이 잡힐 수 있음
* 대부분 `Assets/...`와 `.meta`면 정상
* `Library`, `Temp`, `Logs`, `Obj`, `UserSettings`가 보이면 커밋 중단

### 이후 브랜치 원칙

* Unity 완성본을 유지하는 사소한 수정은 `main`에서 바로 하지 말고 필요하면 별도 브랜치를 만든다.
* Unreal 비교 프로젝트는 이 Unity 저장소에 섞지 않고 별도 프로젝트 / 저장소로 관리한다.
* 포트폴리오 영상 / README만 추가할 경우 작은 문서 브랜치를 사용해도 된다.

---

## 10. 회귀 테스트 체크리스트

에셋 / 애니메이션 하나를 적용할 때마다 관련 항목을 확인한다.  
새 행동 애니메이션을 붙였더라도 전투 판정과 상태가 그대로 작동해야 한다.

### Player 기본 이동

* 정지 시 Idle
* 이동 시작 시 Walk
* 이동 정지 시 Idle 복귀
* 비락온 WASD 이동 방향 회전
* 락온 W/S 전진 / 후진
* 락온 A/D 좌우 이동
* 락온 대각선 4방향
* 일반 Shift Sprint
* 락온 중 Sprint 시 락온 유지
* Shift 해제 후 Enemy 응시 복귀
* Root Motion Off
* 모델이 Player 루트와 따로 이동하지 않음

### Player 공격 3타

* 좌클릭 1회 → Attack1
* 연속 입력 → Attack2
* 다시 입력 → Attack3
* 1타 / 2타 / 3타 데미지 정상
* 검을 뒤로 빼는 준비 동작에서 판정이 켜지지 않음
* 실제 검 궤적과 활성 구간이 크게 어긋나지 않음
* 활성 시간 첫 프레임에 빗나가도 이후 프레임에서 적중 가능
* 한 타격으로 같은 Enemy에 중복 데미지 없음
* 공격 중 이동 잠금
* 공격 종료 후 Idle / Locomotion 복귀
* `SwordHitBox`가 오른손과 검을 따라감

### 일반 방어

* 우클릭 정지 → BlockLocomotion 중앙 자세
* 우클릭 + W/S/A/D → 4방향 방어 이동
* 대각선 이동 시 모션 혼합
* 실제 Enemy 공격 방어 성공 → BlockHit
* 방어 유지 중 BlockHit 후 BlockLocomotion 복귀
* 방어 해제 시 Idle 복귀
* 신체 반응과 카메라 흔들림이 거의 동시에 보임
* 방어 중 `TaoMu Sword_Block` 표시
* 방어 종료 후 일반 검 복귀
* Sprint 중 우클릭은 Block이 아니라 BreakCharge

### 돌파

* Left Shift + Right Mouse
* 입력 순서 무관 발동
* 입력 유지 중 지속
* 입력 해제 시 Recovery
* W/S/A/D 방향 돌파
* 락온 중 후방 / 측면 돌파
* 돌파 애니메이션 반복 정상
* 첫 프레임 고정 / 스케이팅 현상 없음
* `TaoMu Sword_Charge` 표시
* Enemy 명중
* Enemy 공격 중단
* Guard 파괴
* 강한 넉백
* 카메라 흔들림
* 종료 후 일반 검 복귀

### 시신 방패

* E 1회로 시신 집기
* E 재입력으로 시신 폐기
* `IsHoldingCorpse`가 보유 중에만 true
* `IsCorpseRunning`이 Shift + 이동 중에만 true
* 정지 시 왼손 보유 자세
* 일반 이동 시 CorpseShieldLocomotion
* Shift 이동 시 CorpseRun
* Shift 해제 시 CorpseShieldLocomotion 복귀
* 시신 폐기 후 MoveSpeed에 따라 Idle / Locomotion 복귀
* 왼손 Hold Point가 손 움직임을 따라감
* 구르기 제한
* 일반 막기 제한
* 돌파 제한
* Enemy 공격 1회 차단
* 좌클릭 시 시신 소모 후 1타 공격
* 시신 파괴 후 Player 상태 정상 복귀

### Player 일반 피격

* 실제 HP 감소 → `Hit`
* 공격 / 이동 중 피격 시 현재 행동을 끊고 Hit 진입
* 일반 방어 성공 → `BlockHit`만 재생
* 시신 방패 차단 → `Hit` 미재생
* 구르기 무적 성공 → `Hit` 미재생
* 넉백 / HitStun / 카메라 흔들림 정상
* Hit 종료 후 Idle / Locomotion 복귀

### Player 구르기

* 무입력 Space → 현재 Player 정면으로 구르기
* W/A/S/D + Space → 입력 방향을 정면으로 삼아 구르기
* 락온 중 입력 방향으로 끝까지 구르기
* 구르기 도중 입력을 바꿔도 최초 방향 유지
* 구르기 완료 후 락온 Enemy를 다시 바라봄
* 구르기 중 `Roll Forward` 재생
* Start 2 / End 35 적용
* Inspector Dodge Duration `1.10`
* Inspector Invincible Start `0.10`
* Inspector Invincible End `1.03`
* 무적 구간에서 피격 시 HP 감소 없음
* 시신 방패 보유 중 Space 제한 유지
* 전용 좌 / 우 / 후방 Roll 미사용

### Enemy 기존 기능

* Patrol
* AlertStare
* AlertCircle
* Approach
* ApproachAttack
* CloseFaceOff
* CloseCircle
* Flee
* Return
* 1타 / 확률적 2타
* 다수 Enemy 공격권 조율
* 피격
* 넉백
* 공격 중단
* HitStun
* Backstep
* Guard
* 사망
* 락온 자동 해제
* HP Bar 숨김
* CorpseObject 전환

### 에셋 / 모델

* T-Pose 없음
* 모델이 Player 루트를 따라감
* 모델 전방 방향 정상
* 발바닥 위치 정상
* 머티리얼 분홍색 없음
* 모델 Scale 균일
* 일반 / Block / Charge 검 표시가 겹치지 않음
* SwordHitBox 참조 정상
* Console 반복 Error 없음

---

## 11. 새 AI / 작업자에게 주의시킬 것

### 현재 프로젝트는 완료 상태다

이 문서를 읽는 새 AI / 작업자는 현재 Unity 프로토타입을 미완성 프로젝트로 오인하지 말 것.  
2026-06-20 기준 Player / Enemy / 락온 / 시신 방패 / 최소 야외 맵까지 정한 스코프가 완료됐다.

새 기능을 권하기 전에 먼저 다음을 구분한다.

* 실제 회귀 버그
* 포트폴리오 포장 작업
* 선택적 폴리시
* 완전판 게임에서나 필요한 신규 시스템

### 최우선 원칙

* 기존 전투 구조를 대규모 리팩터링하지 말 것
* 실제 이동 / 판정은 기존 코드가 담당
* Animator는 표현만 담당
* Root Motion 중심 구조로 전환하지 말 것
* Rigidbody 속도는 `rb.velocity`가 아니라 `rb.linearVelocity` 사용
* 작동하는 시스템을 에셋이나 예쁜 구조를 이유로 갈아엎지 말 것
* 기능 추가보다 회귀 테스트와 포트폴리오 설명을 우선할 것

### Player 완료 상태

* Idle / Walk 8방향 / Sprint
* 대검 3타
* BreakCharge
* BlockLocomotion / BlockHit
* 일반 Hit
* 자유 방향 Dodge
* CorpseShieldLocomotion / CorpseRun
* 인간형 시신 방패 정렬

Player에 남은 필수 애니메이션은 없다.

### Enemy 완료 상태

* 모델: `HumanM_Dummy_Red - Sword and Shield`
* Combat Idle / 이동 / 선회 / 후퇴
* Attack1 / Attack2
* Guard / Guarded CloseCircle
* Hit / HeavyHit / Death
* Death 후 CorpseObject 전환
* Screen Space 락온 마커

Enemy에 남은 필수 애니메이션은 없다.

### 시신 방패 구조 주의

최종 시신 방패는 단일 HoldPoint에 루트 피벗을 직접 붙이는 구조가 아니다.

```text
CorpseShieldHoldPoint
→ Player 왼손의 실제 접촉 위치

CorpseGripPoint
→ 시신의 견갑 / 흉갑 쪽 잡히는 위치

CorpseShieldPoseAnchor
→ 시신 몸 전체의 회전 / 사선 자세
```

* HoldPoint와 PoseAnchor를 같은 의미로 취급하지 말 것
* 시신 루트 피벗을 다시 손에 직접 맞추지 말 것
* 부모 Scale 상속 때문에 시신이 커지지 않도록 월드 Scale 보정 유지
* 최종 PoseAnchor 회전 기준은 `(-35, 90, 35)`
* 완전한 래그돌 / 발 IK를 필수로 추가하지 말 것
* 시신을 Player 몸 앞에서 상체 방패로 보이게 하고 하체가 사선 아래로 내려가는 실루엣을 유지

### 맵 구조 주의

* 기존 `Ground`가 실제 물리 바닥
* Ground에 Rigidbody를 붙이지 말 것
* 자연물 Prefab Collider가 전투를 방해하면 끄거나 단순 Collider로 교체
* `MapVisualRoot` 아래 Cluster 단위로 정리
* 중앙 전투 공간을 비울 것
* 맵을 전문 레벨 디자인 과제로 확대하지 말 것

### Enemy Animator / Bridge 주의

* `EnemyAnimator` 수정 전 현재 Controller 이름 확인
* PlayerAnimator 원본에서 상태를 삭제하지 말 것
* Controller 복사본과 Animation Clip 원본을 구분
* Player / Enemy가 다른 Start / End를 쓰면 클립을 별도 생성
* `EnemyAnimationBridge.cs` 제거 금지
* `Any State → Hit / HeavyHit / Death` Condition 누락 확인
* `Death` 상태에서 나가는 화살표를 만들지 말 것
* `BlockLocomotion`이 Guard 정지와 이동을 모두 표현하므로 중복 GuardLocomotion을 만들지 말 것

### Enemy 코드 주의

* 1타 / 2타 공격 시간이 분리되어 있음
* `AttackStartSequence` / `CurrentAttackStep` 유지
* `HitReactionSequence` / `HeavyHitReactionSequence` / `DeathReactionSequence` 유지
* 일반 공격은 `TakeDamage`
* 돌파는 `TakeHeavyDamage` 또는 `TakeHeavyHit`
* `IsGuarding`은 제자리 Guard와 Guarded CloseCircle을 모두 포함
* Kinematic Rigidbody에 `linearVelocity` / `angularVelocity`를 설정하지 말 것

### Player 공격 / 방어 주의

* `AttackStartSequence` 제거 금지
* `CurrentComboStep` 유지
* 실제 Enemy 적중 후에만 해당 타격의 추가 검사를 중단
* 판정 크기는 `weaponHitBoxHalfExtents`에서 조정
* Animation Event로 공격 판정 권한을 옮기지 말 것
* `Any State → BlockLocomotion`을 만들지 말 것
* Sprint + 우클릭은 BreakCharge가 우선

### 시신 방패 Animator 전환

```text
Idle / Locomotion → CorpseShieldLocomotion
CorpseShieldLocomotion ↔ CorpseRun
CorpseShieldLocomotion → Idle / Locomotion
```

* E 토글 입력을 E 홀드로 되돌리지 말 것
* `Any State → CorpseShieldLocomotion / CorpseRun`을 만들지 말 것
* Mirror 사용 시 Left / Right 클립 배치를 확인할 것

### 에셋 관련

* 프로젝트는 URP
* 필요한 머티리얼만 개별 변환
* 무료 에셋도 포트폴리오 공개 전 라이선스 확인
* 원본 에셋은 보존하고 수정본을 별도 관리
* 환경 에셋은 완성 Terrain 팩이 아니라 자연물 Prefab 팩임
* `Flowers` 폴더를 `Floors`로 오인하지 말 것

### 입력 경고

Legacy Input Manager deprecation 경고는 현재 입력 전체가 `Input.GetKey`, `Input.GetMouseButton`, `Input.GetAxis` 기반이므로 의도적으로 허용한다.  
경고 하나를 없애기 위해 Active Input Handling을 Input System 전용으로 바꾸면 기존 조작이 대거 멈출 수 있다. 입력 시스템 이전은 별도 프로젝트 수준 작업으로 취급한다.

---

## 12. 사용 에셋 기록표

새 에셋을 Import하거나 포트폴리오에 공개할 때 아래 표를 갱신한다.

| 구분 | 에셋명 | 제작자 | 출처 | 가격 | 라이선스 / 배포 가능 여부 | 사용 위치 | 수정 사항 |
|---|---|---|---|---:|---|---|---|
| Player Model | `PT_Boy_Modular_Free_Pack` 포함 무료 중세 캐릭터 팩 | 확인 필요 | Unity Asset Store | 무료 | 확인 필요 | `PlayerVisualRoot` | URP Lit 변환, Base Map 연결, Humanoid Avatar 사용 |
| Basic Animation | Human Basic Motions FREE | Kevin Iglesias | Unity Asset Store | 무료 | Unity Asset Store EULA 확인 | Player Idle / Walk / Sprint | Root Motion 없는 이동 클립 사용 |
| Combat Animation | Human Melee Animations FREE | Kevin Iglesias | Unity Asset Store | 무료 | Unity Asset Store EULA 확인 | 공격 / 피격 후보 | 필요한 클립만 사용 |
| Combat Animation | RPG_Animations_Pack_FREE | DoubleL | Unity Asset Store | 무료 | Unity Asset Store EULA 확인 | Player 공격 / 돌파 / 방어 / 시신 방패, Enemy 이동 / 공격 / Guard / Hit / Death | Mirror, Player·Enemy 클립 분리, Start / End 절삭, Blend Tree 재구성 |
| Weapon | TaoMu Sword | 확인 필요 | 확인 필요 | 확인 필요 | 확인 필요 | Player Sword | 오른손 슬롯 부착, 일반 / Block / Charge 외형 복사본 사용 |
| Environment | Low Poly Environment - Nature Free - LOWPOLY MEDIEVAL FANTASY SERIES | PolyTope Studio | Unity Asset Store | 무료 | Unity Asset Store EULA 확인 | `MapVisualRoot`, 야외 전투장 외곽 | 나무 / 관목 / 바위 / 풀 Cluster 구성, Y Rotation과 균일 Scale 변형 |
| Ground Material | 프로젝트 내 생성 녹색 URP/Lit Material | 사용자 제작 | 프로젝트 내부 | 무료 | 사용자 소유 | 기존 `Ground` | 회색 물리 바닥을 녹색 평원 비주얼로 변경, Collider 유지 |
| Enemy Model | `HumanM_Dummy_Red - Sword and Shield` (`RPG_Animations_Pack_FREE`) | DoubleL | Unity Asset Store | 무료 | Unity Asset Store EULA 확인 | `EnemyVisualRoot` | 발 높이 보정, 기존 Enemy 루트 / Collider / AI 유지 |
| VFX | 현재 범위 제외 |  |  |  |  |  |  |
| Sound | 현재 범위 제외 |  |  |  |  |  |  |

주의:

* `확인 필요` 항목은 포트폴리오 공개 전 반드시 채울 것
* Asset Store URL과 라이선스 화면을 별도 기록 권장
* 포트폴리오 영상 / 저장소 공개 시 원본 에셋 파일 재배포 가능 여부를 따로 확인
* 수정한 Animator / Material / Prefab은 원본과 구분해 관리

---

## 13. 현재 작업 종료 지점

### 2026-06-20 Unity 최종 종료 기준

#### Player

* 인간형 모델 / Humanoid / URP 정상
* Idle / Walk 8방향 / Sprint
* 대검 3타
* BreakCharge
* BlockLocomotion / BlockHit
* 일반 Hit
* 자유 방향 Dodge
* CorpseShieldLocomotion / CorpseRun
* 인간형 시신 방패 최종 정렬
* Player 필수 그래픽 / 애니메이션 완료

#### Enemy

* 검방 인간형 모델
* Combat Idle / 이동 / 선회 / 후퇴
* Patrol / Approach / Return / CloseCircle
* Attack1 / Attack2와 개별 판정 시간
* Guard / Guarded CloseCircle
* 일반 Hit / 돌파 전용 HeavyHit
* 공격 중 피격 시 공격 중단
* Death / CorpseObject 전환
* Enemy 필수 그래픽 / 애니메이션 완료

#### 락온 / UI / 경고

* HUD Screen Space 락온 마커
* `LockOnTargetPoint` 가슴 위치
* Enemy 사망 / 거리 이탈 자동 해제
* Kinematic Rigidbody 속도 경고 제거
* Legacy Input Manager deprecation 경고만 의도적으로 허용

#### 시신 방패

* `CorpseShieldHoldPoint`: 왼손 접촉점
* `CorpseGripPoint`: 시신 견갑 / 흉갑 잡기점
* `CorpseShieldPoseAnchor`: 시신 전체 회전 기준
* 부모 Scale 역보정으로 시신 비대화 해결
* PoseAnchor 최종 회전 `(-35, 90, 35)`
* 상체가 전면을 가리고 하체가 사선 아래로 내려가는 실루엣 확인

#### 맵

* 기존 Ground 물리 유지
* 녹색 URP/Lit Ground Material
* PolyTope Studio 자연환경 Prefab 사용
* `MapVisualRoot`와 Cluster 구조
* 중앙 전투 공간 확보
* 인게임에서 야외 전투장 표시 확인

### 공식 범위 진행 상황

1. Player 그래픽 에셋 적용 ✅
2. Player 필수 애니메이션 ✅
3. Enemy 그래픽 에셋 적용 ✅
4. Enemy 필수 애니메이션 ✅
5. Screen Space 락온 마커 ✅
6. 인간형 1:1 전투 루프 ✅
7. 시신 방패 인간형 위치 / 회전 보정 ✅
8. 작은 야외 맵 ✅
9. 최종 Play 확인 ✅
10. `asset-integration → main` Merge ✅
11. 사운드 / VFX 현재 범위 제외 ✅

### 2026-06-20 주요 변경

```text
CorpseObject.cs
→ 시신 잡기점 / 자세 Anchor 기반 정렬과 Scale 보정

CorpseShield.cs
→ CorpseShieldPoseAnchor / HoldPoint 기반 시신 위치 갱신

Enemy Hierarchy
→ CorpseGripPoint 추가

Player Hierarchy
→ CorpseShieldPoseAnchor 추가

Ground
→ 녹색 URP/Lit Material 적용

MapVisualRoot
→ 자연물 Cluster 배치
```

에디터 시작 시 발생한 구식 API 관련 경고는 사용처를 최신 API로 교체하여 정리했다. Legacy Input Manager 경고는 기존 입력 구조 유지를 위해 허용한다.

### Git 종료 상태

* 최종 작업 브랜치: `asset-integration`
* Merge 대상: `main`
* 2026-06-20 Merge 완료
* GitHub Desktop Current Branch가 `main`인지 확인
* `Push origin`이 남아 있으면 원격 저장소 반영 후 종료

권장 최종 커밋 메시지:

```text
Complete Unity combat prototype visuals
```

문서만 별도로 커밋할 경우:

```text
Finalize asset integration handoff
```

### 다음 작업

Unity 개발은 종료한다. 다음 작업은 동일 프로젝트의 필수 구현이 아니라 별도 단계다.

1. Unreal 비교 프로토타입
2. 포트폴리오 플레이 영상
3. README / 코드 구조 설명
4. 에셋 출처와 라이선스 정리
5. 필요할 때만 최소 VFX / SFX / 조명 보정

현재 Unity 결과물을 다시 열 때는 명확한 회귀 버그나 포트폴리오 목적이 없는 한 새 기능을 추가하지 않는다.

---

