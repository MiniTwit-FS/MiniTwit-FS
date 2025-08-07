Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/jammy64"

  config.vm.network "forwarded_port", guest: 5000, host: 5000

  config.vm.provision "shell", inline: <<-SHELL
    apt-get update
    apt-get install -y docker.io
    git clone https://github.com/Lukski175/MiniTwit-FS.git
    cd your-minitwit-api
    docker build -t minitwit-api .
    docker run -d -p 5000:80 minitwit-api
  SHELL
end