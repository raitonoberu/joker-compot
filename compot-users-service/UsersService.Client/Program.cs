using Grpc.Net.Client;
using UsersService.Protos;

var channel = GrpcChannel.ForAddress("http://localhost:42069");
var client = new AuthService.AuthServiceClient(channel);

var response1 = client.Register(new RegisterRequest { Email = "joker@mail.ru", Password = "j0k3r", });
Console.WriteLine(response1);
var response2 = client.Login(new LoginRequest { Email = "joker@mail.ru", Password = "j0k3r", });
Console.WriteLine(response2);
