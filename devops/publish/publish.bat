cd ../build
call build.bat
cd ../test
call test-all.bat
cd ../pack
call ./pack.bat
cd ../publish
call ./copy.bat LogoFX.Practices.IoC 2.2.0-rc2 %1
cd ../install
call ./uninstall-global-single.bat LogoFX.Practices.IoC 2.2.0-rc2