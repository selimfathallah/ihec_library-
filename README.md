# IHEC Library - Fichier README

## À propos du projet

IHEC Library est une application de bureau développée spécifiquement pour la communauté du collège "IHEC Carthage". Cette application permet aux étudiants et au personnel administratif de gérer et d'accéder aux ressources académiques de la bibliothèque de manière efficace et intuitive.

## Structure du projet

Ce package contient les éléments suivants :

```
IHEC_Library/
├── dist/                  # Packages d'installation pour différentes plateformes
│   ├── windows/           # Version Windows
│   ├── macos/             # Version macOS
│   ├── linux/             # Version Linux
│   └── source/            # Code source complet
├── documentation/         # Documentation complète
│   ├── README.md          # Vue d'ensemble du projet
│   ├── INSTALLATION.md    # Guide d'installation
│   ├── USER_MANUAL.md     # Manuel d'utilisation
│   ├── TECHNICAL_DOCUMENTATION.md # Documentation technique
│   └── DEPLOYMENT.md      # Guide de déploiement
├── database/              # Scripts et schéma de la base de données
├── src/                   # Code source de l'application
└── build.sh               # Script de compilation
```

## Installation rapide

1. Choisissez le package correspondant à votre système d'exploitation dans le dossier `dist`
2. Suivez les instructions dans le fichier `documentation/INSTALLATION.md`

## Documentation

Consultez les fichiers suivants pour plus d'informations :

- `documentation/README.md` - Vue d'ensemble du projet
- `documentation/INSTALLATION.md` - Guide d'installation détaillé
- `documentation/USER_MANUAL.md` - Manuel d'utilisation complet
- `documentation/TECHNICAL_DOCUMENTATION.md` - Documentation technique
- `documentation/DEPLOYMENT.md` - Guide de déploiement

## Compilation depuis les sources

Si vous souhaitez compiler l'application depuis les sources :

1. Assurez-vous d'avoir installé .NET SDK 6.0 ou supérieur
2. Exécutez le script `build.sh` pour compiler l'application pour toutes les plateformes
3. Les packages compilés seront disponibles dans le dossier `dist`

## Configuration des APIs

L'application utilise deux APIs externes :

1. **Supabase** pour la base de données et l'authentification
   
2. **Gemini** pour le chatbot intelligent
   
## Support

Pour toute assistance technique, veuillez contacter :

- **Email** : support.bibliotheque@ihec.ucar.tn
- **Téléphone** : +216 71 775 948
