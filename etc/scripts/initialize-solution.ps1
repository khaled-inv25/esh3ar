abp install-libs

cd src/Esh3arTech.DbMigrator && dotnet run && cd -



cd src/Esh3arTech.Web && dotnet dev-certs https -v -ep openiddict.pfx -p config.auth_server_default_pass_phrase 


exit 0