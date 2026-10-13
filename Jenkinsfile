// Te same bramki co w .github/workflows/ci.yml, zapisane dla Jenkinsa z wielogałęziowym potokiem
// i Bitbucketem jako źródłem PR-ów (CHANGE_BRANCH / CHANGE_TARGET ustawia wtyczka Bitbucket Branch Source).
// UWAGA: ten plik nie był uruchamiany — autor demo nie ma pod ręką Jenkinsa. Pokazuje, że bramki są
// zwykłymi poleceniami i nie zależą od tego, kto je woła.

pipeline {
    agent { label 'dotnet-10' }
    options { timestamps() }

    stages {
        stage('B1 — kompilacja, ostrzeżenia są błędami') {
            steps { sh 'dotnet build -warnaserror -nologo' }
        }
        stage('B2 — testy') {
            steps { sh 'dotnet test --no-build -nologo --logger trx' }
            post { always { mstest testResultsFile: '**/*.trx' } }
        }
        stage('B3 — każde kryterium zadania ma test') {
            steps { sh 'dotnet run --project tools/Gates --no-build -- tasks "${CHANGE_BRANCH:-$BRANCH_NAME}"' }
        }
        stage('B4 — agent nie zmienia własnych bramek') {
            when { expression { env.CHANGE_BRANCH?.startsWith('agent/') } }
            steps { sh 'dotnet run --project tools/Gates --no-build -- paths "origin/$CHANGE_TARGET"' }
        }
        stage('B5 — opis PR') {
            when { changeRequest() }
            steps {
                // Opis PR pobiera się z API Bitbucketa (pullrequests/{id} → description) do pliku;
                // dane dostępowe z magazynu Jenkinsa, nie z repozytorium.
                sh './ci/pobierz-opis-pr.sh > pr-body.md'   // skryptu nie ma w tym demo
                sh 'dotnet run --project tools/Gates --no-build -- pr pr-body.md'
            }
        }
    }
}
