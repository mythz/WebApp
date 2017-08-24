cat ../apps/rockwind-vfs-aws/web.settings | sed "/bind/s/ .*/ */" > bin/Release/netcoreapp2.0/publish/web.settings

rsync -rave 'ssh -i ~/pem/team-servicestack.pem' bin/Release/netcoreapp2.0/publish/ ec2-user@netcore.io:/home/ec2-user/webtemplates/web

rsync -rave 'ssh -i ~/pem/team-servicestack.pem' ../apps/ ec2-user@netcore.io:/home/ec2-user/webtemplates/apps 
