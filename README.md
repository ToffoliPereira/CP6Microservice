# 📚 Sistema de Gestão de Biblioteca

## 📋 Descrição do Projeto

Aplicação desenvolvida em C# com MySQL, para gerenciar livros, usuários e empréstimos em uma biblioteca. A aplicação permite o cadastro de livros e usuários, controle de empréstimos e devoluções, cálculo de multas por atraso e geração de relatórios.


## 🧩 Estrutura do Projeto

```
📁 Service/
├── 📄 ICacheService.cs      # Interface do serviço de cache
├── 📄 CacheService.cs       # Implementação do serviço de cache
├── 📄 IEmprestimoService.cs      # Interface do serviço de emprestimo
├── 📄 EmprestimoService.cs       # Implementação do serviço de emprestimo
├── 📄 ILivroService.cs      # Interface do serviço de livro
├── 📄 LivroService.cs       # Implementação do serviço de livro
├── 📄 IRelatorioService.cs      # Interface do serviço de relatorio
├── 📄 RelatorioService.cs       # Implementação do serviço de relatorio
├── 📄 IUsuarioService.cs      # Interface do serviço de usuario
├── 📄 UsuarioService.cs       # Implementação do serviço de usuario
└── 📄 Service.csproj        # Dependências do projeto
```

## 🧱 Modelagem do Banco de Dados

```
CREATE DATABASE `cp6`;

USE `cp6`;

CREATE TABLE livros (
	`Isbn` INT NOT NULL auto_increment,
    `Titulo` VARCHAR(45) NOT NULL,
    `Autor` VARCHAR(45) NOT NULL,
    `Categoria` VARCHAR(45) NOT NULL,
    `Status` VARCHAR(45) NOT NULL,
    `DataCadastro` datetime NOT NULL,
PRIMARY KEY(`Isbn`));

CREATE TABLE usuarios (
	`Id` INT NOT NULL auto_increment,
    `Nome` VARCHAR(45) NOT NULL,
    `Email` VARCHAR(45) NOT NULL,
    `Tipo` VARCHAR(45) NOT NULL,
    `DataCadastro` datetime NOT NULL,
PRIMARY KEY(`Id`));

CREATE TABLE emprestimos (
	`IdEmprestimo` INT NOT NULL auto_increment,
    `IsbnLivro` INT NOT NULL,
    `IdUsuario` INT NOT NULL,
    `DataEmprestimo` datetime NOT NULL,
    `DataPrevDevolucao` datetime NOT NULL,
    `DataRealDevolucao` datetime,
    `Status` VARCHAR(45) NOT NULL,
    FOREIGN KEY (IsbnLivro) REFERENCES livros(Isbn),
    FOREIGN KEY (IdUsuario) REFERENCES usuarios(Id),
PRIMARY KEY(`IdEmprestimo`));

CREATE TABLE multas (
	`IdEmprestimo` INT NOT NULL,
    `ValorMulta` numeric(10,2) NOT NULL,
    `Status` VARCHAR(45) NOT NULL,
    FOREIGN KEY (IdEmprestimo) REFERENCES emprestimos(IdEmprestimo)
);
);
```

## ⚙️ Regras de Negócio

- Cadastro de Livros: O livro é identificado pelo ISBN. Possui informações como título, autor, categoria e status (DISPONÍVEL, EMPRESTADO, RESERVADO).

- Cadastro de Usuários: O usuário é identificado por um ID e possui informações como nome, e-mail e tipo (ALUNO, PROFESSOR, FUNCIONÁRIO).

- Empréstimo de Livros:

  - - O usuário pode realizar até 3 empréstimos ativos simultaneamente.

  - - O livro emprestado não pode ser reservado até ser devolvido.

  - - O livro só pode ser emprestado se disponível.

  - - O prazo de empréstimo varia dependendo do tipo de usuário (alunos têm prazo menor que professores).

- Devolução de Livros:

  - - Ao devolver o livro fora do prazo, uma multa é gerada automaticamente.

  - - O cálculo da multa é de R$ 1,00 por dia de atraso.

  - - Usuários com multas pendentes não podem realizar novos empréstimos.

- Relatórios:

  - - Livros mais emprestados.

  - - Usuários com mais empréstimos.

  - - Empréstimos em atraso.

### 🧪 Validações e Erros

- ❌ Livro já emprestado não pode ser reservado ou emprestado novamente.

- ❌ Usuário com mais de 3 empréstimos ativos não pode realizar um novo empréstimo.

- ❌ Tentativa de devolução sem empréstimo ativo.

- ⚠️ Livro abaixo do limite mínimo de estoque (para livros físicos) gera alerta.

- ❌ Produto vencido não pode ser emprestado ou devolvido.

Exceções:

- `LivroIndisponivelException`

- `LimiteEmprestimosExcedidoException`

- `MultaPendenteException`

- `LivroVencidoException`

## 📊 Exemplos de API


POST `/api/livros`

```

{
  "isbn": "978-3-16-148410-0",
  "titulo": "Introdução à Programação",
  "autor": "João Silva",
  "categoria": "TÉCNICO",
  "status": "DISPONÍVEL"
}

```

POST `/api/usuarios`

```

{
  "nome": "Maria Oliveira",
  "email": "maria.oliveira@example.com",
  "tipo": "ALUNO"
}

```

POST `/api/emprestimos`

```

{
  "isbnLivro": "978-3-16-148410-0",
  "idUsuario": 1,
  "dataEmprestimo": "2025-11-01T00:00:00",
  "dataPrevDevolucao": "2025-11-15T00:00:00"
}

```

POST `/api/devolucoes`

```

{
  "idEmprestimo": 123,
  "dataRealDevolucao": "2025-11-18T00:00:00"
}

```

Listar Livros Mais Emprestados

GET `/api/livros/mais-emprestados`

Listar Usuários com Mais Empréstimos

GET `/api/usuarios/mais-emprestimos`

Listar Empréstimos em Atraso

GET `/api/emprestimos/atrasados`

## 🚀 Execução

Clonar o repositório

Configurar conexão no appsettings.json:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CP6;Uid=root;Pwd=senha;"
}
```

Executar:

`dotnet run`

Acessar: http://localhost:5000/api

## 🧾 Entregas

- Etapa	Commit

- Etapa 1: Commit com a mensagem "Etapa 1 - Modelagem do domínio"

- Etapa 2: Commit com a mensagem "Etapa 2 - Implementação das regras de negócio"

- Etapa 3: Commit com a mensagem "Etapa 3 - Validações e tratamento de erros"

- Final: Commit final com documentação e tag "versao-final"

