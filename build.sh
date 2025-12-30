#!/bin/bash

check_dotnet_or_install() {
    echo "checking dotnet environment..."
    if [ -f "$HOME/.dotnet/dotnet" ]; then
        echo "✅ dotnet-cli found in $HOME/.dotnet"
        $HOME/.dotnet/dotnet --list-sdks
        return 0
    fi

    echo "❌ dotnet-cli not found, installing..."
    curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh -c 10.0 -InstallDir $HOME/.dotnet
    rm ./dotnet-install.sh

    if [ -f "$HOME/.dotnet/dotnet" ]; then
        echo "✅ dotnet installed successfully"
        export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
        export DOTNET_ROOT=$HOME/.dotnet
        $HOME/.dotnet/dotnet --list-sdks
    else
        echo "❌ dotnet installation failed"
        exit 1
    fi
}

install_pwsh() {
    if command -v pwsh &> /dev/null; then
        echo "✅ pwsh found"
        return 0
    fi

    echo "installing pwsh..."
    mkdir -p $HOME/.powershell
    wget -qO pwsh.tar.gz https://github.com/PowerShell/PowerShell/releases/download/v7.4.6/powershell-7.4.6-linux-x64.tar.gz
    tar -zxf pwsh.tar.gz -C $HOME/.powershell
    chmod +x $HOME/.powershell/pwsh
    rm pwsh.tar.gz
    export PATH=$PATH:$HOME/.powershell
    
    if command -v pwsh &> /dev/null; then
        echo "✅ pwsh installed successfully"
    else
        echo "❌ pwsh installation failed"
        exit 1
    fi
}

ensure_git_history() {
    echo "ensuring git history..."
    git fetch --unshallow 2>/dev/null || true
    git fetch --all
    git fetch --tags
}

main() {
    check_dotnet_or_install
    install_pwsh
    ensure_git_history

    echo "installing npm packages..."
    cd ./CAO.Client
    npm install
    cd ..
    echo "npm packages installed."

    echo "building and publishing CAO.Client..."
    $HOME/.dotnet/dotnet publish ./CAO.Client -c Release
    echo "CAO.Client built and published."

    echo "exporting git info..."
    pwsh ./export-git-info.ps1 -OutputFile "./CAO.Client/bin/Release/net10.0/publish/wwwroot/timeline.json" -Branch "HEAD"
    echo "git info exported."
}

main "$@"