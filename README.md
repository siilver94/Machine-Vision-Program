# Machine Vision Program

## 프로젝트 개요

이 프로젝트는 **머신 비전(Machine Vision)** 기술을 활용하여 사람이 직접 수행하던 시각적 검사 및 판별 작업을 자동화하는 데 목적이 있습니다.  
산업용 카메라와 비전 처리 SDK, TCP/IP 기반 통신, 데이터베이스 연동 등을 통해 현장의 반복적이고 정밀한 검사 프로세스를 효율적으로 처리할 수 있도록 설계되었습니다.

PC와 카메라는 Ethernet으로, PC와 PLC는 TCP/IP 프로토콜을 사용하여 연결되며, 실시간으로 트리거 신호를 수신하고 검사 결과를 전달합니다.  
사용자는 GUI를 통해 카메라 설정, 촬영 제어, 검사 조건 수정, 결과 확인 등을 수행할 수 있으며, 시스템은 내부적으로 MySQL과 연동하여 결과 데이터를 기록하고 관리합니다.

---

## 주요 기능

- 산업용 카메라 제어 및 이미지 캡처
- 머신 비전 툴을 활용한 이미지 처리 및 분석
- PLC와의 TCP/IP 통신을 통한 신호 수신 및 제어
- 검사 결과를 실시간으로 데이터베이스에 저장
- 사용자 친화적인 Windows Form 기반 GUI 제공
- 검사 임계치 및 결과 스코어 확인 기능 포함

---

## 시스템 구성도

다음은 시스템의 주요 구성 요소 및 통신 구조를 나타낸 다이어그램입니다.

![system_architecture](https://user-images.githubusercontent.com/57824945/143528127-b4189249-e161-415f-83f5-5ed16b33c564.png)

- 카메라 ↔ PC : Ethernet 통신
- PC ↔ PLC : TCP/IP 통신
- PC ↔ DB : MySQL 데이터베이스 연동

---

## 사용 기술

- 개발 언어: C# (.NET Framework)
- 통신 프로토콜: TCP/IP, Ethernet
- 비전 SDK: Cognex VisionPro
- 데이터베이스: MySQL

---

## 개발 후기

스마트 팩토리 및 산업 자동화의 흐름 속에서, 비전 검사의 정확성과 반복성을 확보하는 것은 매우 중요한 과제입니다.  
본 프로젝트는 이러한 요구에 대응하기 위해 사용자 인터페이스부터 하드웨어 연동, 실시간 처리 기능까지 전반적인 구조를 직접 설계하고 구현한 경험을 담고 있습니다.

특히, C#을 이용한 GUI 구성, 비전 SDK 연동, PLC와의 통신 구현, 데이터베이스와의 실시간 동기화 등 여러 기술 요소를 통합하면서,  
소프트웨어 아키텍처와 하드웨어 통신에 대한 이해를 폭넓게 확장할 수 있었습니다.

처음에는 Front-end와 Back-end의 개념조차 명확히 구분하지 못했지만, 본 프로젝트를 통해 각각의 역할과 상호작용 구조를 실질적으로 체험하고 이해하게 되었습니다.  
향후에는 이 프로그램을 기반으로 자동화 품질 검사 솔루션을 더욱 발전시킬 수 있을 것으로 기대하고 있습니다.

---

## 산업적 의의 및 활용 가능성

본 프로그램은 다음과 같은 분야에서 활용될 수 있습니다.

- 공정 내 불량 검출 자동화
- 반복 작업 자동화 및 인적 오류 최소화
- 데이터 기반 품질 분석 및 리포팅
- 스마트 공장 환경 내 연동 가능한 비전 솔루션 구성

![industry_image](https://user-images.githubusercontent.com/57824945/143511709-bbe46469-7333-4918-9f5a-2361da67e293.png)

---

## 향후 개선 방향

- 사용자 권한에 따른 기능 접근 제어
- 검사 결과 로깅 및 이력 관리 기능 강화
- 다양한 카메라 모델과의 호환성 확보
- Web 기반 모니터링 시스템 도입 검토
