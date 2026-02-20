pipeline {
    agent any

    tools {
        nodejs "NodeJS"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build Backend') {
            steps {
                dir('weather-backend') {
                    bat 'dotnet restore'
                    bat 'dotnet build --configuration Release'
                    bat 'dotnet publish -c Release -o publish'
                }
            }
        }

        stage('Build Frontend') {
            steps {
                dir('weather-frontend') {
                    bat 'npm install'
                    bat 'npm run build'
                }
            }
        }

        stage('Archive Backend') {
            steps {
                archiveArtifacts artifacts: 'weather-backend/publish/**'
            }
        }

        stage('Archive Frontend') {
            steps {
                archiveArtifacts artifacts: 'weather-frontend/dist/**'
            }
        }
    }
}