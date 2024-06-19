@echo off
set files="..\..\StrategyUtilities\StrategyUtilitiesClass.cs"
set files=%files%;"..\..\Strategies\AdaptiveRandomStrategy.cs" 
set files=%files%;"..\..\Strategies\MixedStrategy.cs"
set files=%files%;"..\..\Strategies\SearchStrategy.cs"
set files=%files%;"..\..\Strategies\SmartRandomStrategy.cs"
set files=%files%;"..\..\Strategies\StupidShortWordsStrategy.cs"
set files=%files%;"..\..\Strategies\ShortWordsStrategy.cs"
set files=%files%;"..\..\Strategies\RandomStrategy.cs"
set files=%files%;"..\..\ProductionsGameCore\Simplifier.cs"
set files=%files%;"..\..\ProductionsGameCore\SimplifiedWord.cs"
set files=%files%;"..\..\ProductionsGameCore\GameSettings.cs"
set files=%files%;"..\..\ProductionsGameCore\SimplifiedProductionGroup.cs"
set files=%files%;"..\..\ProductionsGameCore\Bank.cs"
set files=%files%;"..\..\ProductionsGameCore\RandomSettings.cs"
set files=%files%;"..\..\ProductionsGameCore\ProductionGroup.cs"
set files=%files%;"..\..\ProductionsGameCore\Grammatic.cs"
set files=%files%;"..\..\ProductionsGameCore\PrimaryMove.cs"
set files=%files%;"..\..\ProductionsGameCore\Move.cs"
set files=%files%;"..\..\ProductionsGame\Strategy.cs"
set files=%files%;"..\..\ProductionsGame\Game.cs"
set files=%files%;"..\..\ProductionsGame\Parameters.cs"
set files=%files%;"..\..\ProductionsGame\RandomProvider.cs"

if not exist "code" md "code"

for %%f in (%files%) do (
    chcp 65001
    cmd /d /u /c type %%f > .tmp
    chcp 1251
    cmd /d /c type .tmp > "code/%%~nxf"
)
del .tmp
set /p asd="Hit enter to continue"
