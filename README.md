# Stack

좌우로 왕복하는 블록을 탭해 쌓는 Unity 3D 하이퍼캐주얼 게임입니다. 어긋난 부분은 잘려 나가고, 쌓을 곳이 없으면 게임 오버입니다.

**▶ [웹에서 플레이하기](https://gaaamj.github.io/Stack/)**

## 조작과 규칙

- 화면을 **클릭 / 탭**하면 움직이던 블록이 멈춰 아래 블록 위에 쌓입니다.
- 겹치지 않은 부분은 잘려 떨어지고, 블록 폭이 그만큼 줄어듭니다.
- 거의 정확히 맞추면 **Perfect**로 보정(snap)되어 자르지 않고, 연속하면 콤보가 쌓입니다.
- 블록이 아래 블록과 전혀 겹치지 않으면 게임 오버입니다. 점수는 쌓은 층수이고, 최고점은 브라우저/기기에 저장됩니다.
- 블록은 X축과 Z축을 번갈아 움직이며, 시작하는 쪽(- 또는 +)은 매번 무작위입니다.

## 구현한 것

- 판정: Miss / Perfect / Cut, 잘린 조각 낙하
- 조작감: 입력 즉시 판정, Perfect 허용 오차 snap, 착지 바운스, Perfect 링, 콤보에 따라 올라가는 효과음, 짧은 hit-stop
- 연출: 층마다 바뀌는 파스텔 블록 색, 그라데이션 + 반짝이 별 배경, 게임 오버 시 탑 전체가 보이는 줌아웃
- UI: 점수, 콤보, 시작 안내, 게임 오버 화면과 재시작 버튼
- 이미지·사운드 에셋 없이 코드로 생성했습니다(배경 그라데이션, 별, 효과음).

## 코드 구조

| 스크립트 | 역할 |
|---|---|
| `GameManager` | 상태(Ready / Playing / GameOver), 점수·콤보·최고점, 입력, 모듈 호출 조율 |
| `StackController` | 블록 생성·이동·정지, Miss / Perfect / Cut 판정, 잘린 조각 생성 |
| `Block` | 블록 하나의 색칠, 바운스 연출, 낙하 |
| `CameraFollow` | 탑 높이 추적, 게임 오버 줌아웃 |
| `ThemeController` | 층별 블록 색, 조명 색 보간 |
| `BackgroundController` | 그라데이션 배경, 반짝이 별 |
| `FeedbackFX` | Perfect 링, 사운드, hit-stop |
| `UIController` | 점수·콤보·게임 오버 화면 |

자세한 설계와 판단 근거는 [`Docs/Stack_기획서.md`](Docs/Stack_기획서.md), [`Docs/CHANGE_LOG.md`](Docs/CHANGE_LOG.md)에 있습니다.

## 실행 방법

- Unity **6000.3.26f1**으로 프로젝트를 열고 `Assets/Scenes/SampleScene`을 재생합니다. (URP, Input System, TextMeshPro 사용)
- 웹 빌드는 `gh-pages` 브랜치에 올라가 있고 GitHub Pages로 서비스됩니다. `main` 브랜치에는 소스만 있습니다.
