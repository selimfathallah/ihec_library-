#!/bin/bash

# Script de compilation et de packaging pour IHEC Library
# Ce script compile l'application pour Windows, macOS et Linux

echo "=== Début du processus de compilation et de packaging ==="
echo "Date: $(date)"

# Vérifier si .NET SDK est installé
if ! command -v dotnet &> /dev/null; then
    echo "Erreur: .NET SDK n'est pas installé. Veuillez l'installer avant de continuer."
    exit 1
fi

# Définir les variables
PROJECT_DIR="$(pwd)"
SRC_DIR="$PROJECT_DIR/src/IHECLibrary"
DIST_DIR="$PROJECT_DIR/dist"
DOCS_DIR="$PROJECT_DIR/documentation"
VERSION="1.0.0"

# Créer les dossiers de distribution s'ils n'existent pas
mkdir -p "$DIST_DIR/windows"
mkdir -p "$DIST_DIR/macos"
mkdir -p "$DIST_DIR/linux"
mkdir -p "$DIST_DIR/source"

echo "Dossiers de distribution créés."

# Nettoyer et restaurer les packages
echo "Nettoyage et restauration des packages..."
dotnet clean "$SRC_DIR"
dotnet restore "$SRC_DIR"

# Compiler pour Windows
echo "Compilation pour Windows..."
dotnet publish "$SRC_DIR" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$DIST_DIR/windows/IHECLibrary"

# Compiler pour macOS
echo "Compilation pour macOS..."
dotnet publish "$SRC_DIR" -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o "$DIST_DIR/macos/IHECLibrary"

# Compiler pour Linux
echo "Compilation pour Linux..."
dotnet publish "$SRC_DIR" -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "$DIST_DIR/linux/IHECLibrary"

# Copier la documentation
echo "Copie de la documentation..."
cp -r "$DOCS_DIR"/* "$DIST_DIR/windows/IHECLibrary/"
cp -r "$DOCS_DIR"/* "$DIST_DIR/macos/IHECLibrary/"
cp -r "$DOCS_DIR"/* "$DIST_DIR/linux/IHECLibrary/"

# Créer une archive du code source
echo "Création de l'archive du code source..."
mkdir -p "$DIST_DIR/source/IHECLibrary"
cp -r "$SRC_DIR"/* "$DIST_DIR/source/IHECLibrary/"
cp -r "$DOCS_DIR"/* "$DIST_DIR/source/IHECLibrary/"

# Créer les archives
echo "Création des archives..."
cd "$DIST_DIR/windows" && zip -r "../IHECLibrary-$VERSION-windows.zip" "IHECLibrary"
cd "$DIST_DIR/macos" && zip -r "../IHECLibrary-$VERSION-macos.zip" "IHECLibrary"
cd "$DIST_DIR/linux" && zip -r "../IHECLibrary-$VERSION-linux.zip" "IHECLibrary"
cd "$DIST_DIR/source" && zip -r "../IHECLibrary-$VERSION-source.zip" "IHECLibrary"

echo "=== Processus de compilation et de packaging terminé ==="
echo "Les packages sont disponibles dans le dossier: $DIST_DIR"
