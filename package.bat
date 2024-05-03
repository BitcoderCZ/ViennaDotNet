@ECHO OFF

if not exist "build" mkdir build

if not exist "build\ApiServer" mkdir build\ApiServer
xcopy /q /y ViennaDotNet.ApiServer\bin\Release\net8.0\publish\ApiServer.exe build\ApiServer
xcopy /q /y ViennaDotNet.ApiServer\bin\Release\net8.0\publish\aspnetcorev2_inprocess.dll build\ApiServer
xcopy /q /y ViennaDotNet.ApiServer\bin\Release\net8.0\publish\e_sqlite3.dll build\ApiServer
xcopy /q /y /s ViennaDotNet.ApiServer\bin\Release\net8.0\publish\data build\ApiServer


if not exist "build\Buildplate" mkdir build\Buildplate
xcopy /q /y ViennaDotNet.Buildplate\bin\Release\net8.0\win-x64\publish\BuildplateLauncher.exe build\Buildplate
xcopy /q /y ViennaDotNet.Buildplate\bin\Release\net8.0\win-x64\publish\e_sqlite3.dll build\Buildplate

if not exist "build\EventBusServer" mkdir build\EventBusServer
xcopy /q /y ViennaDotNet.EventBus.Server\bin\Release\net8.0\publish\EventBusServer.exe build\EventBusServer

if not exist "build\ObjectStoreServer" mkdir build\ObjectStoreServer
xcopy /q /y ViennaDotNet.ObjectStore.Server\bin\Release\net8.0\publish\ObjectStoreServer.exe build\ObjectStoreServer

if not exist "build\TappablesGenerator" mkdir build\TappablesGenerator
xcopy /q /y ViennaDotNet.TappablesGenerator\bin\Release\net8.0\publish\TappablesGenerator.exe build\TappablesGenerator