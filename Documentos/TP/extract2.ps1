$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
$path = 'C:\Users\larag\Documents\GitHub\Grupo-N-6-Plataforma-Entradas\Documentos\TP\Casos de uso (1).docx'
$doc = $word.Documents.Open($path)
foreach ($para in $doc.Paragraphs) {
    Write-Host $para.Range.Text
}
$doc.Close()
$word.Quit()
