#include <stdio.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <netdb.h>
#include <signal.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <unistd.h>
#include <string.h>

#define BUFSIZE 1024





const int bufsize = 1024;
int client_socket;
char* payload = NULL;



// ctrl+c signal handler
// frees malloced memory and closes socket
void sigint_handler(int sig) {

    close(client_socket);
    if (payload != NULL)
        free(payload);
    printf("\nCaught SIGINT signal, exiting gracefully.\n");
    exit(0);
}




// controlls arguments
// return 0 if the arguments are wrong
int argument_controll(int argc, char* argv[], char** host, char** port, char** mode) {

    if (argc != 7) {
        fprintf(stderr, "Invalid number of arguments\n");
        fprintf(stderr, "Usage: ipkcpc -h <host> -p <port> -m <mode>\n");
        return 1;
    }

    for (int i = 1; i < argc; i += 2) {
        if (strcmp(argv[i], "-h") == 0) {
            *host = argv[i + 1];
        } 
        else if (strcmp(argv[i], "-p") == 0) {
            *port = argv[i + 1];
        } 
        else if (strcmp(argv[i], "-m") == 0 && (strcmp(argv[i + 1], "tcp") == 0 || strcmp(argv[i + 1], "udp") == 0)) {
            *mode = argv[i + 1];
        }
        else {
            fprintf(stderr, "Invalid argument: %s\n", argv[i]);
            return 1;
        }
    }

    //printf("Host: |%s|\n", *host);
    //printf("Port: |%s|\n", *port);
    //printf("Mode: |%s|\n", *mode);
    return 0;
}



// void function for UDP connection
void udp_client(char* host, int port) {
    int bytes_sent, bytes_received;
    socklen_t serverlen;
    struct hostent *server;
    struct sockaddr_in server_address;

    char buf[bufsize];

    const char *server_ip = host;
    int server_port = port;

    //bzero((char *) &server_address, sizeof(server_address));
    /* ititalisation of struct server_address */

    memset((char *) &server_address, 0, sizeof(server_address));
    server_address.sin_family = AF_INET;
    server_address.sin_addr.s_addr = inet_addr(server_ip);
    server_address.sin_port = htons(port);

    /*
    const char *server_hostname = host;
    int server_port = port;

    if ((server = gethostbyname(server_hostname)) == NULL) {
        fprintf(stderr, "ERROR: no such host as %s\n", server_hostname);
        exit(1);
    }

    bzero((char *) &server_address, sizeof(server_address));
    server_address.sin_family = AF_INET;
    bcopy((char *) server->h_addr, (char *) &server_address.sin_addr.s_addr, server->h_length);
    server_address.sin_port = htons(server_port);
    */


    // initialization of scket
    if ((client_socket = socket(AF_INET, SOCK_DGRAM, 0)) < 0) {
        fprintf(stderr, "ERROR: could not create socket\n");
        exit(1);
    }
    
    int working = 1;
    
    // while loop for comunication with server 
    while (working) {
        bzero(buf, bufsize);
        // printf("Enter a message: ");
        fgets(buf, bufsize, stdin);
        uint8_t opcode = 0;
        uint8_t buffer_len = strlen(buf);

        int total_len = 2 + buffer_len;
        payload = malloc(total_len);

        //creates byte structure to send
        memcpy(payload, &opcode, sizeof(opcode));
        memcpy(payload + sizeof(opcode), &buffer_len, sizeof(buffer_len));

        memcpy(payload + sizeof(opcode) + sizeof(buffer_len), buf, buffer_len);

        serverlen = sizeof(server_address);
        //sending the bytes
        bytes_sent = sendto(client_socket, payload, total_len, 0, (struct sockaddr *) &server_address, serverlen);
        if (bytes_sent < 0) {
            fprintf(stderr, "ERROR: could not send message\n");
            exit(1);
        }

        free(payload);
        payload = NULL;

        bzero(buf, bufsize);
        // recieves response from server
        bytes_received = recvfrom(client_socket, buf, bufsize, 0, (struct sockaddr *) &server_address, &serverlen);
        if (bytes_received < 0) {
            fprintf(stderr, "ERROR: could not receive message\n");
            exit(1);
        }
        // print_bits(buf, bytes_received);

        uint8_t recv_opcode = buf[0];
        uint8_t status_code = buf[1];
        uint8_t recv_buffer_len = buf[2];


        

        payload = malloc(recv_buffer_len);
        memcpy(payload, buf + 3, recv_buffer_len);

        // printing recieved bytes
        // if error occured
        if(status_code == 1) {
            fprintf(stderr, "ERR: ");
            for (int i = 0; i < recv_buffer_len; i++) {
                fprintf(stderr, "%c", payload[i]);
            }
            working = 0;
        }
        // if error did not occure
        else{
            for (int i = 0; i < recv_buffer_len; i++) {
                printf("%c", payload[i]);
            }
            printf("\n");
        }

        free(payload);
        payload = NULL;
    }
    close(client_socket);
}


// void function for tcp client
void tcp_client(char* host, int port) {
    int bytes_sent, bytes_received;
    socklen_t serverlen;
    struct hostent *server;
    struct sockaddr_in server_address;

    char buf[bufsize];


    const char *server_ip = host;
    int server_port = port;

    // initialisation of structure server address
    bzero((char *) &server_address, sizeof(server_address));
    server_address.sin_family = AF_INET;
    server_address.sin_addr.s_addr = inet_addr(server_ip);
    server_address.sin_port = htons(port);



    /*
    int server_port = port;
    const char *server_hostname = host;
    if ((server = gethostbyname(server_hostname)) == NULL) {
        fprintf(stderr,"ERROR: no such host as %s\n", host);
        return 1;
    }

    //najdenie IP adresy serveru a inicializace struktury server_address 
    bzero((char *) &server_address, sizeof(server_address));
    server_address.sin_family = AF_INET;
    bcopy((char *)server->h_addr, (char *)&server_address.sin_addr.s_addr, server->h_length);
    server_address.sin_port = htons(port);

    */

    // initialize socket
    if ((client_socket = socket(AF_INET, SOCK_STREAM, 0)) <= 0)
    {
        fprintf(stderr,"ERROR: socket");
        exit(EXIT_FAILURE);
    }

    // connection
    if (connect(client_socket, (const struct sockaddr *) &server_address, sizeof(server_address)) != 0)
    {
        fprintf(stderr,"ERROR: connect");
        exit(EXIT_FAILURE);
    }

    fgets(buf,  BUFSIZE, stdin);

    // reads firs message and controlls if it is HELLO
    if (strcmp(buf, "HELLO\n") != 0)
    { 
        fprintf(stderr,"ERROR: HELLO");
        exit(EXIT_FAILURE);
    }

    if (send(client_socket, buf, strlen(buf), 0) < 0) {
        fprintf(stderr,"ERROR: SENDING");
        exit(EXIT_FAILURE);
    }
    bzero(buf, BUFSIZE);

    if (recv(client_socket, buf, BUFSIZE, 0) < 0) {
        fprintf(stderr,"ERROR in recvfrom");
        exit(EXIT_FAILURE);
    }

    if (strcmp(buf, "HELLO\n") != 0) {
        fprintf(stderr,"ERROR while establishing connection");
        exit(EXIT_FAILURE);
    }
    printf("%s", buf);

    int connected = 1;
    // while cycle for communication 
    while (connected) {

        bzero(buf, BUFSIZE);
        fgets(buf, BUFSIZE, stdin);

        if (strcmp(buf, "BYE\n") == 0)
        { 
            connected = 0;
        }
   
        if (send(client_socket, buf, strlen(buf), 0) < 0) {
            fprintf(stderr,"ERROR: SENDING");
            exit(EXIT_FAILURE);
        }
            
            bzero(buf, BUFSIZE);

        if (recv(client_socket, buf, BUFSIZE, 0) < 0) { fprintf(stderr,"ERROR in recvfrom");
            exit(EXIT_FAILURE);
        }

        if (strcmp(buf, "BYE\n") == 0)
        { 
            connected = 0;
        }

        printf("%s", buf);
    }
    close(client_socket);
}



int main(int argc, char *argv[]){ 
    char *host;
    char *port;
    char *mode;

    signal(SIGINT, sigint_handler);


	int client_socket, port_number, bytestx, bytesrx;
    socklen_t serverlen;

    const char *server_hostname;
    struct hostent *server;
    struct sockaddr_in server_address;
    char buf[BUFSIZE];

    // controlls arguments
    if (argument_controll(argc, argv, &host,  &port, &mode)) {
        return 1;
    }

    server_hostname = host;
    port_number = atoi(port);

    ////////////////////////////////////////////////////////////////////////////////////
    // based on given mode decides which function to call
    if (strcmp(mode, "udp") == 0) {
        udp_client(host, port_number);
    }
    else {
        tcp_client(host, port_number);
    }

    return 0;
}

