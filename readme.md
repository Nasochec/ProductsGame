# Вам представляется приложение разработанное для моделирования "Игры в подстановки" и проверки стратегий для неё. #




## Краткое описание стратегий ##
### 1. RandomStrategy - случайная стратегия ###
Разработана первой, как самая простая - ход выбирается случайно.

### 2. StupidShortWordsStrategy - "глупая" стратегия коротких слов ###
Стратегия, которая старается как можно быстрее получить слово. Стратегия основана на том, чтобы построить некую метрику оценки продукций и выводов, чтобы как можно быстрее (за меньшее количество шагов), получить слово. Метрика вывода проста - количество нетерминалов в выводе. Метрика продукции A->X где X- строка из N U T, равна метрике вывода X. Метрика группы продукций A->X1|X2|... равна минимуму из метрик продукций A->X1 ... 

Далее в кажой группе продукций находится лучшая продукция. Далее выпавшая продукция применяется, если возможно, к слову, содержащему указаный нетерминал, с наименьшей метрикой, если удалось применить, то из банка выбирается продукция с наименьшей метрикой, и также применяется к выводу с наименьшей метрикой, это повторяется пока не останется продукций которые можно применить.

Стратегия названа глупой, поскольку её легко обмануть, добавив продукции с непораждающими нетерминалами, или рекурсивные продукции.

### 3. ShortWordsStrategy - Стратегия коротких слов ###
Стратегия призванная избежать проблем присущих предыдущей стратегии. Для этого разработана другая метрика. Её описание довольно сложно поэтому подробно оно будет представлено уже в тексте курсовой. Кротко для терминальных слов она 1, для строк содержащих непораждающие нетерминалы она 0, в ином случае она от 0 до 1, чем меньше эта метрика, тем за большее количество ходов вывод можно привести к теримнальному слову(учитывая шансы выпадения групп продукций). Метрика же продукций и групп продукций определяется аналогично. Формирование хода также проходит аналогично.

### 4. SearchStrategy - переборная стратегия ###
Стратегия получила своё название из изначальной задумки. Изначально она была предназначена для грамматики, в которой мал шанс выпадения продукции позволяющей создать новый вывод(продукция S->X). Идея такова: в момент выпадения продукции позволяющей создать новый вывод, мы его создаём и перебираем все возможные слова, которые можно получить используя продукции из банка, и выбираем из них лучшее(предполагалось получать сразу терминальное слово и выбирать из них самое длинное). Но Была обнаружена пролема: из за большого количества групп продукций(11 штук) и продукций в каждой группе, перебор получался глубоким и шёл слишком долго.Поэтому было принято решение ограничить глубину перебора небольшим числом. Но тогда возникает вопрос: *как оценить слова?* И в этот момент пришла идея использовать метрику из предыдущей стратегии. Таким образом перебираются несколько шагов в перёд, получается слово с лучшей метрикой, полученный ход применяется, и далее повторяем пока есть какие-либо ходы.

### 5. Smart Random Strategy - умная случайная стратегия ###
Стратегия основана на подбирании случайного хода, но её отличие от простой случайной стратегии, где все выборы делаются с равной аероятностью, тут мы будем делать выбор основываясь на весах.
В качестве весов возмём метрику из стратегии Short Word Strategy.

### 6. Поиск циклов -> рюкзак -> дособираение слова до конца###

### ?. AnotherSearchStrategy ###
Смысл стратегии: изнчально берём слово с лучшей метрикой, из него строим такое слово(обозначим B), которое имеет максимальное возможное количество нетерминалов, которе можно за 1 шаг превратить в терминалы и необродимые для этого продукции есть в банке в необходимом количестве. далее стратегией аналогичной предыдущей мы строим все выводы приводящие нас в положение которое имеет меньшее количество нетерминалов чем в слове B. 

Вопрос: в лове B используются продукции из банка, как не использовать те же подукции в простроении выводов приводящих к слову В?
