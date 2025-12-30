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

check_pwsh_or_install() {
    echo "checking pwsh environment..."
    if command -v pwsh &> /dev/null; then
        echo "✅ pwsh found"
        return 0
    fi

    echo "❌ pwsh not found, installing..."

    $HOME/.dotnet/dotnet tool install --global PowerShell

    export PATH=$PATH:$HOME/.dotnet/tools

    if command -v pwsh &> /dev/null; then
        echo "✅ pwsh installed successfully"
    else
        echo "❌ pwsh installation failed"
        exit 1
    fi
}

main() {
    check_dotnet_or_install
    check_pwsh_or_install

    echo "installing npm packages..."
    cd ./CAO.Client
    npm install
    cd ..
    echo "npm packages installed."

    echo "building and publishing CAO.Client..."
    $HOME/.dotnet/dotnet publish ./CAO.Client -c Release
    echo "CAO.Client built and published."

    echo "exporting git info..."
    pwsh ./export-git-info.ps1 ./CAO.Client/bin/Release/net10.0/publish/wwwroot/timeline.json
    echo "git info exported."
}

main "$@"