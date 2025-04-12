@echo off
@echo Tutorial on setting up an https static host on AWS with custom domain name
@echo https://www.stormit.cloud/blog/setup-an-amazon-cloudfront-distribution-with-ssl-custom-domain-and-s3/
echo All paths used are relative the project folder
set projectPath=%~dp0
set projectCSProjFileName=%projectPath%\SpawnDev.BlazorJS.Test.csproj
set publishFolder=%~dp0bin\Publish
set publishFolderRoot=%publishFolder%\wwwroot
set publishFrameworkFolder=%publishFolderRoot%\_framework
set publishFrameworkFolderCompat=%publishFolderRoot%\_frameworkCompat
set publishCompatFolder=%~dp0bin\PublishCompat
set publishCompatFolderRoot=%publishCompatFolder%\wwwroot
set publishCompatFrameworkFolder=%publishCompatFolderRoot%\_framework
set awsBucket=blazorjs.spawndev.com
set distroId=EMBSO45DEQI30
@echo on

@echo Make sure javascript files are marked as application/javascript files and not text/plain
@echo Fix registry setting everytime because Window's update may overwrite it.
@REG ADD HKCU\Software\Classes\.js /v "Content Type" /t REG_SZ /d "application/javascript" /f
@echo Must be ran in cmd instance for rmdir to work (PowerShell rmdir is different)

REM https://github.com/dotnet/runtime/issues/78872

REM SIMD enabled build
rmdir /Q /S "%publishFolder%"
dotnet publish --nologo --configuration Release "%projectCSProjFileName%" -p:WasmEnableSIMD=true -p:BlazorWebAssemblyJiterpreter=true --output "%publishFolder%"

REM SIMD disabled build
REM Compat build could also use  -p:WasmEnableExceptionHandling=false but WasmException handling is more common than SIMD
rmdir /Q /S "%publishCompatFolder%"
dotnet publish --nologo --no-restore --configuration Release "%projectCSProjFileName%" -p:WasmEnableExceptionHandling=false -p:WasmEnableSIMD=false -p:BlazorWebAssemblyJiterpreter=false --output "%publishCompatFolder%"

@echo Copying compat build
xcopy /I /E /Y "%publishCompatFrameworkFolder%" "%publishFrameworkFolderCompat%"

echo Ready to upload (press any key to continue or Ctrl-C to cancel)
pause
@IF NOT [%awsBucket%] == [] (
    aws s3 rm s3://%awsBucket% --recursive
    aws s3 sync "%publishFolderRoot%\." s3://%awsBucket%
    @IF NOT [%distroId%] == [] (
        aws cloudfront create-invalidation --distribution-id %distroId% --paths "/*"
    )
)
@pause
