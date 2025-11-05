# 🏪 Sistema de Gestão de Estoque

## 📋 Descrição do Projeto

Aplicação desenvolvida em C# com MySQL, para gerenciar produtos perecíveis e não perecíveis, controlando estoque, movimentações e alertas automáticos.


## 🧩 Estrutura do Projeto

```
📁 Service/
├── 📄 ICacheService.cs      # Interface do serviço de cache
├── 📄 CacheService.cs       # Implementação do serviço de cache
└── 📄 Service.csproj        # Dependências do projeto
```

## 🧱 Modelagem do Banco de Dados

```
CREATE DATABASE CP5;
USE CP5;

CREATE TABLE Produtos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(45),
    Categoria VARCHAR(45),
    PrecoUnitario NUMERIC(10,2),
    QtdMin NUMERIC(10),
    DataCriacao DATETIME DEFAULT NOW()
);

CREATE TABLE Estoque (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdProduto INT,
    Tipo VARCHAR(45),
    Qtd NUMERIC(10),
    DataMovimentacao DATETIME DEFAULT NOW(),
    Lote NUMERIC(10),
    DataValidade DATETIME,
    FOREIGN KEY (IdProduto) REFERENCES Produtos(Id)
);
```

## ⚙️ Regras de Negócio

- Produtos perecíveis exigem lote e data de validade.

- Quantidades não podem ser negativas.

- Saídas verificam estoque suficiente.

- Atualização automática do saldo após movimentação.

- Alerta para estoque abaixo do mínimo.

- Relatórios de produtos vencendo em até 7 dias.

### 🧪 Validações e Erros

- ❌ Produto perecível sem validade → erro

- ❌ Movimentação negativa → erro

- ❌ Saída maior que o estoque → erro

- ⚠️ Produto abaixo do mínimo → alerta

Exceções:

EstoqueInsuficienteException
ProdutoVencidoException
QuantidadeInvalidaException

## 📊 Exemplos de API

```
POST /api/produtos

{
  "nome": "Leite Integral",
  "categoria": "PERECIVEL",
  "precoUnitario": 6.50,
  "qtdMin": 10
}


POST /api/estoque

{
  "idProduto": 1,
  "tipo": "SAIDA",
  "qtd": 5,
  "lote": 123,
  "dataValidade": "2025-11-10"
}
```

## 🚀 Execução

Clonar o repositório

Configurar conexão no appsettings.json:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CP5;Uid=root;Pwd=;"
}
```

Executar:

`dotnet run`

Acessar: http://localhost:5000/api

## 🧾 Entregas

- Etapa	Commit

- Etapa 1	Etapa 1 - Modelagem do domínio

- Etapa 2	Etapa 2 - Implementação das regras de negócio

- Etapa 3	Etapa 3 - Validações e tratamento de erros

- Final	Etapa 4 - Documentação final