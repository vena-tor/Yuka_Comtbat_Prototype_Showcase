## Combat Prototype

완결한 개인 웹소설 《인류애 없는 용사님》(https://novel.naver.com/best/list?novelId=1190475) 을 기반으로 Unity와 C#으로 제작한 3D 액션 전투 프로토타입이다.

원작 주인공 유카의 대검 전투, 돌파, 시신 방패 등의 전투 콘셉트를 플레이어가 직접 조작할 수 있는 게임 시스템으로 구현했다.

상용 게임 수준의 그래픽 완성도보다, 소설 속 전투 설정을 캐릭터 조작·공격 판정·적 AI·애니메이션·피격 반응이 연결된 하나의 전투 루프로 변환하는 데 집중했다.

* 개발 기간: 2026.05.26 ~ 2026.06.20
* 개발 인원: 1인
* 원작: 개인 완결 웹소설 《인류애 없는 용사님》(https://novel.naver.com/best/list?novelId=1190475)
* 엔진: Unity
* Render Pipeline: URP
* 언어: C#

---

## Project Overview

플레이어와 검방형 Enemy가 소형 야외 전투장에서 대결하는 액션 전투 프로토타입이다.

실제 이동과 전투 판정은 Rigidbody 및 C# 코드가 담당하고, Animator는 해당 결과를 시각적으로 표현하도록 역할을 분리했다.

Root Motion에 의존하지 않으며, 기존 전투 로직을 유지한 상태에서 인간형 모델과 애니메이션을 단계적으로 통합했다.

---

## Original IP

이 프로젝트는 개인 창작 완결 웹소설《인류애 없는 용사님》(https://novel.naver.com/best/list?novelId=1190475) 의 설정과 전투 콘셉트를 기반으로 제작했다.

원작은 단순한 배경 설정이 아니라, 캐릭터의 전투 방식과 기술 우선순위를 결정하는 스토리보드이자 디자인 기준으로 활용했다.

현재 프로토타입에 반영된 주요 원작 요소는 다음과 같다.

* 대검을 사용하지만 느리지 않은 빠른 근접 전투
* 적의 공격과 방어 태세를 무너뜨리는 돌파
* 사망한 적을 전투 자원으로 다시 사용하는 시신 방패
* 강한 캐릭터를 자동 전투가 아니라 플레이어의 직접 조작으로 표현하는 방향

현재 버전은 원작 전체를 게임화한 작품이 아니라, 원작의 핵심 전투 콘셉트가 실제 3D 액션 시스템으로 성립하는지를 검증한 프로토타입이다.

---

## Core Features

### Player

* 카메라 기준 WASD 이동
* 자유 시점 카메라
* 화면형 락온 시스템
* 락온 상태 전후좌우 이동
* 3타 공격 콤보 및 입력 예약
* 공격별 개별 판정 시간
* 방향 입력 기반 구르기
* 구르기 무적 구간
* 이동 가능한 방어
* 실제 방어 성공 시 BlockHit 반응
* 방향 전환이 가능한 돌파 공격
* 돌파를 통한 Enemy 공격 중단 및 Guard Break
* 사망한 Enemy를 사용하는 시신 방패
* 시신 방패 상태 전용 이동 및 달리기

### Enemy

* 순찰 및 Player 탐지
* 원거리 응시와 선회
* 접근 및 근거리 대치
* 근거리 측면 이동
* 확률적 연속 공격
* 확률적 Backstep과 Guard
* 방패를 든 상태의 근거리 선회
* 일반 피격 반응
* 돌파 전용 HeavyHit 반응
* 공격 중 피격 시 공격 중단
* Death 애니메이션
* 사망 후 시신 오브젝트 전환

### Combat Presentation

* Player / Enemy Animation Bridge
* Rigidbody 실제 이동 방향 기반 Blend Tree
* 일반 Hit / HeavyHit / Death 반응 분리
* Screen Space 락온 마커
* Player / Enemy HP UI
* 소형 야외 전투 맵

---

## Implementation Highlights

### Code-Driven Combat

공격 판정과 전투 상태는 Animator가 아니라 C# 코드가 관리한다.

애니메이션은 전투 결과를 표현하며, 실제 판정은 각 공격의 Windup, Active, Recovery 시간에 따라 별도로 동작한다.

### Animation Bridge

전투 코드와 Animator를 직접 강하게 결합하지 않고, 별도의 Bridge 스크립트를 통해 다음 값을 전달한다.

* 이동 속도와 로컬 이동 방향
* 공격 시작 번호
* 현재 콤보 단계
* Guard 상태
* 일반 피격
* HeavyHit
* Death

이를 통해 전투 코드의 권한을 유지하면서 애니메이션을 교체할 수 있도록 구성했다.

### Enemy State-Based AI

Enemy는 단순히 Player를 향해 직선 이동하지 않고, 다음 상태를 조합해 행동한다.

```text
Patrol
→ Alert
→ Approach
→ Close Face Off
→ Circle / Attack / Guard / Backstep
→ Hit / HeavyHit / Death
```

다수전 확장을 고려하여 동시에 공격할 수 있는 Enemy 수를 제한하는 공격권 조율 구조도 포함되어 있다.

### Corpse Shield

사망한 Enemy 오브젝트를 별도 시신으로 교체하지 않고, 동일 오브젝트를 `CorpseObject` 상태로 전환한다.

Player는 가까운 시신을 들어 공격을 한 번 막을 수 있으며, 직접 공격하거나 다시 입력하면 시신을 소모한다.

시신의 실제 잡는 지점과 몸 전체의 자세 기준을 분리하여, 견갑을 붙잡고 하체가 사선으로 처지는 형태로 정렬했다.

---

## Controls

| Input                           | Action        |
| ------------------------------- | ------------- |
| WASD                            | 이동            |
| Mouse                           | 카메라 회전        |
| Left Shift                      | 달리기           |
| Left Mouse Button               | 공격 / 콤보       |
| Right Mouse Button              | 방어            |
| Left Shift + Right Mouse Button | 돌파            |
| Space                           | 구르기           |
| E                               | 시신 방패 집기 / 폐기 |
| Tab / Middle Mouse Button       | 락온 전환         |

---

## Current Status

현재 프로젝트의 공식 목표였던 다음 범위는 완료되었다.

* Player 인간형 그래픽 및 전투 애니메이션
* Enemy 인간형 그래픽 및 전투 애니메이션
* 공격 판정과 애니메이션 타이밍 연결
* 락온과 HP UI
* Enemy AI와 1:1 전투 루프
* 돌파 및 HeavyHit
* Death 및 시신 전환
* 인간형 시신 방패
* 소형 야외 전투 맵

---

## Known Limitations

본 프로젝트는 전투 시스템 검증을 목적으로 한 프로토타입이다.

* Player와 Enemy 모델은 임시 무료 에셋이다.
* 애니메이션 전환 일부가 매끄럽지 않다.
* 타격 VFX와 사운드는 현재 범위에서 제외했다.
* 캐릭터와 맵의 시각적 통일성은 제한적이다.
* 상용 게임 수준의 레벨 디자인과 환경 연출은 포함하지 않는다.
* 완전한 래그돌이나 시체 물리는 구현하지 않았다.

---

## Assets

프로젝트에는 Unity Asset Store에서 제공되는 무료 모델 및 애니메이션 에셋이 사용되었다.

주요 사용 에셋:

* PT_Boy_Modular_Free_Pack
* Human Basic Motions FREE
* Human Melee Animations FREE
* RPG Animations Pack FREE
* Low Poly Environment - Nature Free

각 에셋의 저작권은 원 제작자에게 있으며, 본 저장소에서는 Unity Asset Store EULA를 따른다.

---

## Development Notes

이 프로젝트는 완결한 웹소설의 전투 장면과 캐릭터 설정을 실제 조작 가능한 게임 시스템으로 옮길 수 있는지 검증하기 위해 시작했다.

Unity 및 3D 액션 전투 개발을 처음 시작한 상태에서 제작했으며, 기초 예제를 순차적으로 구현하기보다 원작에 필요한 전투 기능을 작은 단위로 추가했다. 각 기능을 구현할 때마다 Play 테스트와 Git 커밋을 반복하는 방식으로 진행했다.

기존 구조를 대규모로 교체하지 않고, 작동 중인 전투 코드 위에 모델·Animator·AI·피격 반응을 단계적으로 결합하는 것을 원칙으로 삼았다.
