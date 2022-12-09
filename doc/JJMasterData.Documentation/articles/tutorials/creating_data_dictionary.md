# Creating a Data Dictionary from a table 


# Creating a Data Dictionary from scratch

Você poderá dar início ao processo de criação através do link /pt-br/DataDictionary. 
O ícone New irá permitir que você faça a criação das suas tabelas de acordo com o dicionário criado.

Com o JJMasterData será possível fazer a importação ou criação da sua tabela e a organização da mesma para dicionários e posteriormente exibidos em metadados.

TUTORIAL SEM TABELA

Para a criação da estrutura de dados direto pelo JJMasterData será necessário deixar o campo “import field” desmarcado, inserir o nome da tabela a ser criada e clicar em Next. Preenchido o campo Table Name e clicado em Next, você irá acessar a janela de Entidade. Aqui você irá encontra os seguintes itens:

Dictionary name:  Será o nome que irá representar o seu metadado.
	
Table Name: Nome da tabela onde seus dados ficarão armazenados.

Get procedure Name: Nome da procedure que Irá realizar a leitura dos dados.

Set Procedure Name: Nome da procedure que Irá realizar aescrita dos dados.

Title:  Aqui você irá preencher com o título que será exibido dentro do Form

Subtitle: Aqui você irá preencher com o subtítulo que será exibido dentro do Form

Info: Informações fixas que irão ser mostradas para o desenvolvedor que esteja desenvolvendo a aplicação.

O próximo passo é acessar o campo Fields, local aonde você poderá preencher e formatar os dados que irão ser exibidos na criação da nova tabela a partir dos metadados. É obrigatório o preenchimento dos seguintes campos: FieldName, Filter, DataBehavior, Data Type, Size,  Required, Pk, Identify.

FieldName: Este nome será apresentado somente dentro do banco de dados, será relacionado somente dentro do banco, não será exibido ao usuário final.

Label: Este campo deve ser preenchido de acordo com o nome que deseja ser exibido no título da coluna, por exemplo, caso seja uma coluna de emails, o título deverá conter esse nome ou algo que tenha relação.

DafaultValue: Uma expressão que irá retornar um valor padrão caso o valor dentro do banco de dados seja nulo.

Filter: Campo que indica um tipo de filtro que será executado na procedure de get

DataBehavior: Comportamento de dados

Real- Será usand em Get e Set;
Virtual - Será utilizado somente em Set;

ViewOnly - Será usado somente em Get;

DataType: Tipo dos dados que serão recebidos para tabela.

Size: Quantidade caracteres e quando utilizado como Varchar será o tamanho a ser reservado dentro do banco de dados.

Required: Irá definir a obrigatoriedade do preenchimento do campo.

PK: Definir se é ou não a chave primária.

Identity:  Se o campo será auto incremento, por exemplo, se ele irá retornar um campo de forma automática de dentro do banco de dados. 

Help Description: Mensagem a ser exibida para o usuário com a finalidade auxilia-lo.


Após o preenchimento de todos os itens obrigatórios em Fields, você deverá executar outra ação antes da tabela ser definitivamente  criada. Com todos os campos obrigatórios preenchidos você irá acessar a opção More na lateral direita da tela e clicar em Get Script, dessa forma os dados preenchidos na aba Fields serão convertidos para um script SQL e será exibido para você. Após a exibição dos scripts, você terá a opção de executar a stored procedure, caso não haja procedures a serem executadas, basta clicar em Run All e salvar a página clicando na parte inferior. Pronto, agora sua tabela está criada. Ao clicar no botão Exit, ao lado do botão Entidade, você poderá retornar ao local onde as tabelas estão sendo exibidas. Ao localizar a sua tabela, clique no ícone a direita ao lado do botão de editar, assim você irá visualizar o botão Render, ele permitirá que você veja como será a exibição final.