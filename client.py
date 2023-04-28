import socket
import time
import sys


class ChatClient:
    def __init__(self, id):
        self.servers = [8888, 2222]
        self.current_server_index = 0
        self.client = None
        self.is_connected = False
        self.id = id

    def connect(self):
        current_server = self.servers[self.current_server_index]
        try:

            self.client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.client.connect(('127.0.0.1', current_server))
            self.is_connected = True
            # print(f'Connected to {current_server}')
        except socket.error as ex:
            # print(f'Connection error: {ex}')
            time.sleep(0.1)
            self.reconnect()

    def disconnect(self):
        self.client.close()
        self.client = None
        self.is_connected = False
        # print('Disconnected')

    def reconnect(self):
        self.current_server_index += 1
        self.current_server_index %= len(self.servers)
        self.disconnect()
        self.connect()

    def send_message(self):
        if not self.is_connected:
            self.connect()

        while True:
            try:
                choice = input(
                    'Enter your choice (rock, paper, scissors) or q for exit: ')
                if choice == 'q':
                    break
                if choice not in ('rock', 'paper', 'scissors'):
                    print('Invalid choice. Please try again.')
                    continue

                # замените 'host' и port на соответствующие значения
                # result = self.client.connect_ex(
                #     ('127.0.0.1', self.servers[self.current_server_index]))

                # # print(self.id)
                # if result == 0:
                #     self.reconnect()

                toServer = f'{id},{choice}'

                self.client.send(toServer.encode())

                data = self.client.recv(1024)

                if not data:
                    self.reconnect()
                    self.client.send(toServer.encode())
                    data = self.client.recv(1024)

                message = data.decode()
                print(message)
                #blocks = message.split(',')
                #result = blocks[0]
                #serverChoice = blocks[1]

                #print(f'Result: {result}\nServer choice: {serverChoice}\n')
            except socket.error as ex:
                # print(f'Error sending message: {ex}')
                self.reconnect()

        self.disconnect()


if __name__ == '__main__':
    id = sys.argv[1] if len(sys.argv) > 1 else 56
    client = ChatClient(id)
    client.send_message()
