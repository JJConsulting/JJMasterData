# Creating a Data Dictionary from a table 

Para a criação da estrutura de dados com JJMasterData através de uma tabela já existente em seu banco de dados, é necessário que o campo “Import field” esteja marcado. O campo Table Name será preenchido com o nome da tabela já existente. Preenchido o table Name e marcado o campo Import fields, é só clicar em next e será criada sua estrutura de dados.

Depois de criada, você poderá acessar a aba Fields e você irá ver os campos da sua tabela preexistente, sendo possível edita-los. Também podendo acrescentar novos campos utilizando o ícone de adicionar, ao lado da barra de pesquisa, porém é importante que seja feito o alter table após a adição do novo campo.

# Creating a Data Dictionary from scratch

Você poderá dar início ao processo de criação através do link /pt-br/DataDictionary. 
O ícone New irá permitir que você faça a criação das suas tabelas de acordo com o dicionário criado.

Com o JJMasterData será possível fazer a importação ou criação da sua tabela e a organização da mesma para dicionários e posteriormente exibidos em metadados.

TUTORIAL SEM TABELA

Para a criação da estrutura de dados direto pelo JJMasterData será necessário deixar o campo “import field” desmarcado, inserir o nome da tabela a ser criada e clicar em Next. Preenchido o campo Table Name e clicado em Next, você irá acessar a janela de Entidade. Aqui você irá encontra os seguintes itens:

**Entity**

- *Dictionary name:*  Será o nome que irá representar o seu metadado.
	
- *Table Name:* Nome da tabela onde seus dados ficarão armazenados.

- *Get procedure Name:*  Nome da procedure que Irá realizar a leitura dos dados. Você poderá visualizar a procedure criada clicando em *more*, *Get Script* e depois *Get Procedure*.

- *Set Procedure Name:* Nome da procedure que Irá realizar aescrita dos dados. Você poderá visualizar a procedure criada clicando em *more*, *Get Script* e depois *Set Procedure*.

- *Title:*  Aqui você irá preencher com o título que será exibido dentro do Form. Será possível habilitar ou não a visualização do título através das configurações internas.

- *Subtitle:* Aqui você irá preencher com o subtítulo que será exibido dentro do Form. Da mesma forma que o title, o subtitle também poderá ser habilitado ou não.

- *Info:* Informações fixas que irão ser mostradas para o desenvolvedor que esteja desenvolvendo a aplicação.

**Fields**

O próximo passo é acessar o campo Fields, local aonde você poderá preencher e formatar os dados que irão ser exibidos na criação da nova tabela a partir dos metadados. É obrigatório o preenchimento dos seguintes campos: FieldName, Filter, DataBehavior, Data Type, Size,  Required, Pk, Identify.

- *FieldName:* Este nome será apresentado somente dentro do banco de dados, será relacionado somente dentro do banco, não será exibido ao usuário final.

- *Label:* Este campo deve ser preenchido de acordo com o nome que deseja ser exibido no título da coluna, por exemplo, caso seja uma coluna de emails, o título deverá conter esse nome ou algo que tenha relação.

- *DafaultValue:* Uma expressão que irá retornar um valor padrão caso o valor dentro do banco de dados seja nulo.

- *Filter:* Campo que indica um tipo de filtro que será executado na procedure de get

- *DataBehavior:* Comportamento de dados

    - Real: Será usado em Get e Set;

    - Virtual: Será utilizado somente em Set;

    - ViewOnly: Será usado somente em Get.

- *DataType:* Tipo dos dados que serão recebidos para tabela.

- *Size:* Quantidade caracteres e quando utilizado como Varchar será o tamanho a ser reservado dentro do banco de dados.

- *Required:* Irá definir a obrigatoriedade do preenchimento do campo.

- *PK:* Definir se é ou não a chave primária, é importante que você preencha um dos itens da sua tabela como chave primária.

- *Identity:*  Se o campo será auto incremento, por exemplo, se ele irá retornar um campo de forma automática de dentro do banco de dados. 

- *Help Description:* Mensagem a ser exibida para o usuário com a finalidade auxilia-lo.

Após o preenchimento de todos os itens obrigatórios em Fields, você deverá executar outra ação antes da tabela ser definitivamente  criada. Com todos os campos obrigatórios preenchidos você irá acessar a opção More na lateral direita da tela e clicar em Get Script, dessa forma os dados preenchidos na aba Fields serão convertidos para um script SQL e será exibido para você. Após a exibição dos scripts, você terá a opção de executar a stored procedure, caso não haja procedures a serem executadas, basta clicar em Run All e salvar a página clicando na parte inferior. Pronto, agora sua tabela está criada. Ao clicar no botão Exit, ao lado do botão Entidade, você poderá retornar ao local onde as tabelas estão sendo exibidas. Ao localizar a sua tabela, clique no ícone a direita ao lado do botão de editar, assim você irá visualizar o botão Render, ele permitirá que você veja como será a exibição final.

**Panels**
     Permite separar os campos do dicionários em paineis. Mas somente para as ações de adicionar, editar e visualizar.

- General
    - Layout: É Maneira com que o painel será reinderizado.
    - Expanded By Default: Está opção irá definir se o painel criado será iniciado maximizado por minimizado por padrão.
    - Available Field: Serão os itens que não serão exibidos como paineis.
    - Selected Fields: Ao mudar um item da sua tabela para o campo Selected fields, você verá que ao acessar sua tabela e tentar realizar alguma ação entre adicionar, editar e visualizar um item, ele irá ser exibido como painel.
- Adavanced
    - Visible Expression: seeref
    - Enable Expression: seeref
    - CssClass: Classe Css do campo.

**Indexes and Relationships**
-Ambos os itens serão utilizados para gerar informações dos seus metadados. Você poderá adicionar essas informações clicando no botão *New*.

**Actions**
- Grid
    - View: Este ícone será exibido ao lado dos itens de sua tabela, é possível encontra-lo ao utilizar a visualização prévia de sua tabela. Ao clicar no botão *View*, será exibido a linha escolhida de forma detalhada.
    - Edit: Este ícone será exibido ao lado dos itens de sua tabela, é possível encontra-lo ao utilizar a visualização prévia de sua tabela. Ao clicar no botão *Edit*, será possível alterar informarções já registradas anteriormente em sua tabela de dados.
    - Delete: Este ícone será exibido ao lado dos itens de sua tabela, é possível encontra-lo ao utilizar a visualização prévia de sua tabela. Está opção irá deletar a linha de informações desejada.
- Toolbar
    - Insert: Permite adicionar um novo dado na sua tabela.
    - config: Permite que você acesse as configurações de layout para o usuário, por exemplo, records per page and show table border.
    - Export: Permite selecionar a exportação dos dados presentes na tabela. Você poderá importar apenas os dados visíveis na tela ou todos os dados de sua tabela, é permitido que a exportação seja feita em arquivos pdf, csv, excel e txt. Caso a exportação seja feita em PDF, é necessário que seja habilitado o plugin de pdf (LINK PARA DOC PLUGIN PDF).
    - Import: Este item será exibido como upload para o usuário, permitindo a importação de dados para sua tabela através de arquivos txt, csv e log. Você poderá clicar no botão Help Para visualizar a formatação de upload do arquivo.
    - Refresh: A opção refresh irá atualizar sua tabela de dados, para caso haja alguma mudança para ser exibida.
    - Legend: A legenda é utilizada para auxiliar a descrição do dado dentro de sua tabela, por exemplo, você poderá criar a legenda para uma coluna de sexo, em que é possível atribuir a descrição Woman, Man e na sequência associar com cores e ícones. Você pode ver uma descrição completa de como criar sua legenda através do link (LINK PARA DATA_ITEM_LEG)
    - Sort:
    - Filter:
    - Log:

**API**

- Dentro desta aba será possível editar cada verbo responsável pelas permissões http dentro da REST API.
    - ApplyUseridOn:
    - JsonFormat: Está opção irá definir e modificar a formatação do arquivo Json. Ao utilizar a opção default, ficará definido o padrão já escolhido pelo usuário, entretando a opção LowerCase irá formatar o arquivo para letras minúsculas.
    - Sync:



