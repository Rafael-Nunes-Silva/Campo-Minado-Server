# Campo-Minado-Server

Esse é o servidor necessário para o modo multiplayer do <a href="https://github.com/Rafael-Nunes-Silva/Campo-Minado">Campo-Minado</a>.



# Como Rodar
O processo de execução do servidor é bem simples e funciona tanto em LAN quanto através da internet.

Ao iniciar o servidor, você deverá passar a porta pela qual o servidor irá "escutar" por jogadores.

Após isso, você será perguntado qual deve ser o limite de salas abertas simultaneamente. (Caso a maquina rodando o servidor possua uma CPU com poucos threads ou pouquissima memoria RAM, é aconselhavel que não coloque um limite muito alto).

Após isso o servidor deve exibir uma mensagem indicando que foi iniciado, e esperará por tentativas de conexões por parte dos jogadores.



# Possiveis problemas
## Hardware
Em maquinas com hardware extremamente limitado (geralmente com dificuldades para rodar somente o Windows), pode ser que o servidor não consiga manter-se conectado aos jogadores, mesmo com números pequenos de conexões.

## Firewall
Esse problema é mais comum quando se tenta hostear o servidor em uma rede e jogadores em outras redes tentam fazer a conexão.

O problema é que o Firewall (da maquina em que o servidor está rodando e/ou do roteador em que essa máquina está conectada) pode estar bloqueando a entrada de pacotes na porta que o servidor está rodando.
