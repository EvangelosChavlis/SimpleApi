using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using SimpleApi.Models;
using SimpleApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

//ConfigureServices
builder.Services.AddSingleton<CustomerRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {new OpenApiSecurityScheme{Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }}, new List<string>()}
    });
});

// builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Customer>());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser()
    .Build();
});

var app = builder.Build();

//Configure

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/customers",
[ProducesResponseType(200, Type = (typeof(Customer)))]
(CustomerRepository repo) =>
{
    return Results.Ok(repo.GetAll());
}).AllowAnonymous();

app.MapGet("/customers/{id}", (CustomerRepository repo, Guid id) =>
{
    var customer = repo.GetById(id);
    return customer is not null ? Results.Ok(customer) : Results.NotFound();
});

app.MapPost("/customers", (CustomerRepository repo,/* IValidator<Customer> validator,*/ Customer customer) =>
{
    // var validationResult = validator.Validate(customer);

    // if (!validationResult.IsValid)
    // {
    //     var errors = validationResult.Errors.Select(x => x.ErrorMessage);
    //     return Results.BadRequest(errors);
    // }

    repo.Create(customer);
    return Results.Created($"/customers/{customer.Id}", customer);
});

app.MapPut("/customers/{id}", (CustomerRepository repo, Guid id, Customer updatedCustomer) =>
{
    var customer = repo.GetById(id);

    if (customer is null)
        return Results.NotFound();

    repo.Update(updatedCustomer);
    return Results.Ok(updatedCustomer);
});

app.MapDelete("/customers/{id}", (CustomerRepository repo, Guid id) =>
{
    repo.Delete(id);
    return Results.Ok();
});

app.Run();