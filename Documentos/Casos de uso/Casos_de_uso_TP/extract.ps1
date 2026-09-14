Add-Type -AssemblyName DocumentFormat.OpenXml
$doc = [DocumentFormat.OpenXml.Packaging.WordprocessingDocument]::Open('C:\Users\larag\Documents\GitHub\Grupo-N-6-Plataforma-Entradas\Documentos\TP\Casos de uso (1).docx', $false)
$text = $doc.MainDocumentPart.Document.Body.InnerText
$doc.Close()
$text
