# TicketSystem (.NET 8, MVC, SQLite)

## Rodando local
```bash
cd TicketSystem
dotnet run
# http://localhost:5000
# Admin Geral padrão: admin@local / Admin123$
```

## Publicação
```bash
dotnet publish -c Release -o out
```

## Observações
- Ao registrar, o usuário recebe o papel **User**.
- **SuperAdmin** (admin geral) consegue definir o papel de qualquer usuário como *User*, *Admin* ou *SuperAdmin* em **Admin Geral → Gerenciar Usuários**.
- *Admin* e *SuperAdmin* veem todos os tickets, podem responder e alterar status.
- *User* vê apenas seus próprios tickets.
