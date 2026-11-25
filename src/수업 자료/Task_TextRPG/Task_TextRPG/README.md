# 프로젝트 아키텍처 소개


## 프로젝트 아키텍처 구성
: 3계층 아키텍처 사용 (Framework, Model, Core)
<br>

1. **Framework Layer(프레임워크 계층)**
: 프로젝트 전반에 사용되는 클래스 모음
<br>

2. **Model Layer(모델 게층)**
: 게임 도메인 모음
<br>

3. **Core Layer(코어 계층)**
: 게임 핵심 구성 클래스 모음
<br>

----

## 프로젝트 아키텍처 참조 방식
: 단방향 참조 방식(Framework > Model > Core)