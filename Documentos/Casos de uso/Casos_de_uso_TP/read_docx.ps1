\ = New-Object -ComObject Word.Application
\.Visible = \False
\.DisplayAlerts = 0
\ = 'C:\Users\larag\Documents\GitHub\Grupo-N-6-Plataforma-Entradas\Documentos\TP\Casos de uso (1).docx'
\ = \.Documents.Open(\)
foreach (\ in \.Paragraphs) {
    Write-Host \.Range.Text
}
\.Close()
\.Quit()
