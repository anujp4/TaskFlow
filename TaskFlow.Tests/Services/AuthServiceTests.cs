using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Mappings;
using TaskFlow.Application.Services;
using TaskFlow.Core.Entities;

namespace TaskFlow.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IMapper _mapper;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            _mockConfiguration = new Mock<IConfiguration>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            // Setup JWT configuration
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["Secret"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
            jwtSection.Setup(x => x["Issuer"]).Returns("TaskFlowAPI");
            jwtSection.Setup(x => x["Audience"]).Returns("TaskFlowClient");
            jwtSection.Setup(x => x["ExpiryInMinutes"]).Returns("60");

            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

            _authService = new AuthService(_mockUserManager.Object, _mockConfiguration.Object, _mapper);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_ReturnsSuccess()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                UserName = "johndoe",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);

            _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.UserName))
                .ReturnsAsync((User?)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(registerDto.Email);
            result.Data.Token.Should().NotBeNullOrEmpty();

            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com",
                UserName = "johndoe",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            var existingUser = new User
            {
                Email = registerDto.Email,
                UserName = "existinguser"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("email already exists");
            result.Data.Should().BeNull();

            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "john.doe@example.com",
                Password = "Test@1234"
            };

            var user = new User
            {
                Id = "user123",
                Email = loginDto.Email,
                UserName = "johndoe",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(loginDto.Email);
            result.Data.Token.Should().NotBeNullOrEmpty();

            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, loginDto.Password), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "john.doe@example.com",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Email = loginDto.Email,
                UserName = "johndoe",
                IsActive = true
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid credentials");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var userId = "user123";
            var user = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                UserName = "johndoe",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(userId);
            result.Data.Email.Should().Be(user.Email);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }
    }
}