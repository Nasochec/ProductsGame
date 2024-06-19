@echo off
@REM set files="..\..\StrategyUtilities\StrategyUtilitiesClass.cs"
@REM set files=%files%;"..\..\Strategies\AdaptiveRandomStrategy.cs" 
@REM set files=%files%;"..\..\Strategies\MixedStrategy.cs"
@REM set files=%files%;"..\..\Strategies\SearchStrategy.cs"
@REM set files=%files%;"..\..\Strategies\SmartRandomStrategy.cs"
@REM set files=%files%;"..\..\Strategies\StupidShortWordsStrategy.cs"

set files="ProductionsGameLauncher\bin\Debug\GUIStrategy.dll"
set files=%files%;"ProductionsGameLauncher\bin\Debug\ProductionsGame.dll"
set files=%files%;"ProductionsGameLauncher\bin\Debug\ProductionsGameCore.dll"
set files=%files%;"ProductionsGameLauncher\bin\Debug\ProductionsGameLauncher.exe"
set files=%files%;"ProductionsGameLauncher\bin\Debug\Strategies.dll"
set files=%files%;"ProductionsGameLauncher\bin\Debug\StrategyUtilities.dll"
if not exist "binaries" md "binaries"

for %%f in (%files%) do (
    copy %%f "binaries/%%~nxf"
    @REM chcp 65001
    @REM cmd /d /u /c type %%f > .tmp
    @REM chcp 1251
    @REM cmd /d /c type .tmp > 
)
set /p asd="Hit enter to continue"
