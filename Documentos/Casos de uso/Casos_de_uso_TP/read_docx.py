import docx
doc = docx.Document(r'C:\Users\larag\Documents\GitHub\Grupo-N-6-Plataforma-Entradas\Documentos\TP\Casos de uso (1).docx')
for i, para in enumerate(doc.paragraphs):
    print(f'{i}: {para.text}')
