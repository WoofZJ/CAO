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
        $HOME/.dotnet/dotnet --list-sdks
    else
        echo "❌ dotnet installation failed"
        exit 1
    fi
}

main() {
    check_dotnet_or_install
    cd ./CAO.Client
    npm install
    cd ..
    $HOME/.dotnet/dotnet publish ./CAO.Client -c Release
}

main "$@"