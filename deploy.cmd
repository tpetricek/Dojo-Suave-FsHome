SET ARTIFACTS=%~dp0%..\artifacts
SET DEPLOYMENT_SOURCE=%~dp0%.
SET DEPLOYMENT_TARGET=%ARTIFACTS%\wwwroot
SET NEXT_MANIFEST_PATH=%ARTIFACTS%\manifest
SET PREVIOUS_MANIFEST_PATH=%ARTIFACTS%\manifest

.paket\paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

packages\FAKE\tools\FAKE.exe Deploy --fsiargs build.fsx --from:%~dp0% --to:%~dp0%..\wwwroot