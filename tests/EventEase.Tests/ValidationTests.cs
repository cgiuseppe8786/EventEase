using System.ComponentModel.DataAnnotations;
using EventEase.Models;

namespace EventEase.Tests;

public class ValidationTests
{
    private static List<string> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results.SelectMany(r => r.MemberNames).ToList();
    }

    private static RegistrationModel ValidRegistration() => new()
    {
        FullName = "Maria Rossi",
        Email = "maria.rossi@example.it",
        Phone = "+39 333 1234567",
        AcceptTerms = true
    };

    [Fact]
    public void Registration_ValidModel_HasNoErrors()
    {
        Assert.Empty(Validate(ValidRegistration()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("M")]
    [InlineData("Maria <script>")]
    [InlineData("123 Rossi")]
    public void Registration_InvalidName_IsRejected(string name)
    {
        var model = ValidRegistration();
        model.FullName = name;

        Assert.Contains(nameof(RegistrationModel.FullName), Validate(model));
    }

    [Theory]
    [InlineData("D'Angelo Anna-Maria")]
    [InlineData("José Müller")]
    public void Registration_NamesWithAccentsAndApostrophes_AreAccepted(string name)
    {
        var model = ValidRegistration();
        model.FullName = name;

        Assert.Empty(Validate(model));
    }

    [Theory]
    [InlineData("")]
    [InlineData("maria")]
    [InlineData("maria@localhost")]
    [InlineData("maria rossi@example.it")]
    [InlineData("maria@@example.it")]
    public void Registration_InvalidEmail_IsRejected(string email)
    {
        var model = ValidRegistration();
        model.Email = email;

        Assert.Contains(nameof(RegistrationModel.Email), Validate(model));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registration_BlankPhone_IsTreatedAsMissing(string phone)
    {
        var model = ValidRegistration();
        model.Phone = phone;

        Assert.Null(model.Phone);
        Assert.Empty(Validate(model));
    }

    [Fact]
    public void Registration_InvalidPhone_IsRejected()
    {
        var model = ValidRegistration();
        model.Phone = "chiamami";

        Assert.Contains(nameof(RegistrationModel.Phone), Validate(model));
    }

    [Fact]
    public void Registration_TermsNotAccepted_IsRejected()
    {
        var model = ValidRegistration();
        model.AcceptTerms = false;

        Assert.Contains(nameof(RegistrationModel.AcceptTerms), Validate(model));
    }

    [Fact]
    public void Event_PastDateAndZeroCapacity_AreRejected()
    {
        var ev = new EventItem
        {
            Id = 1,
            Name = "Evento",
            Location = "Milano",
            Category = "Meetup",
            Date = DateTime.Now.AddDays(-1),
            Capacity = 0
        };

        var errors = Validate(ev);

        Assert.Contains(nameof(EventItem.Date), errors);
        Assert.Contains(nameof(EventItem.Capacity), errors);
    }

    [Fact]
    public void Event_ValidEvent_HasNoErrors()
    {
        var ev = new EventItem
        {
            Id = 1,
            Name = "Evento",
            Location = "Milano",
            Category = "Meetup",
            Date = DateTime.Now.AddDays(10),
            Capacity = 50
        };

        Assert.Empty(Validate(ev));
    }
}
