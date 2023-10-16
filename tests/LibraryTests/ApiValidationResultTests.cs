using NUnit.Framework;
using System.Linq;

namespace LibraryTests
{
    public class ApiValidationResultTests
    {
        [Test]
        public void ValidStatusWithDifferentDataAreConsideredEqual()
        {
            //Arrange
            var valid1 = ApiValidationResultStatus.Valid;
            var valid2 = ApiValidationResultStatus.Valid with { Data = new { result = 1 } };
            var valid3 = ApiValidationResultStatus.Valid with { Data = "Hello" };


            //Act Assert
            Assert.That(valid1 == ApiValidationResultStatus.Valid, Is.True);
            Assert.That(valid2 == ApiValidationResultStatus.Valid, Is.True);
            Assert.That(valid3 == ApiValidationResultStatus.Valid, Is.True);
            Assert.That(valid1 == valid2, Is.True);
            Assert.That(valid1 == valid3, Is.True);
            Assert.That(valid2 == valid3, Is.True);
        }

        [Test]
        public void NotFoundStatusWithDifferentErrorsAreConsideredEqual()
        {
            //Arrange
            var status1 = ApiValidationResultStatus.NotFound;
            var status2 = ApiValidationResultStatus.NotFound with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.NotFound with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == ApiValidationResultStatus.NotFound, Is.True);
            Assert.That(status2 == ApiValidationResultStatus.NotFound, Is.True);
            Assert.That(status3 == ApiValidationResultStatus.NotFound, Is.True);
            Assert.That(status1 == status2, Is.True);
            Assert.That(status1 == status3, Is.True);
            Assert.That(status2 == status3, Is.True);
        }

        [Test]
        public void InvalidStatusWithDifferentErrorsAreConsideredEqual()
        {
            //Arrange
            var status1 = ApiValidationResultStatus.Invalid;
            var status2 = ApiValidationResultStatus.Invalid with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.Invalid with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == ApiValidationResultStatus.Invalid, Is.True);
            Assert.That(status2 == ApiValidationResultStatus.Invalid, Is.True);
            Assert.That(status3 == ApiValidationResultStatus.Invalid, Is.True);
            Assert.That(status1 == status2, Is.True);
            Assert.That(status1 == status3, Is.True);
            Assert.That(status2 == status3, Is.True);
        }


        [Test]
        public void ForbiddenStatusWithDifferentErrorsAreConsideredEqual()
        {
            //Arrange
            var status1 = ApiValidationResultStatus.Forbidden;
            var status2 = ApiValidationResultStatus.Forbidden with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.Forbidden with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == ApiValidationResultStatus.Forbidden, Is.True);
            Assert.That(status2 == ApiValidationResultStatus.Forbidden, Is.True);
            Assert.That(status3 == ApiValidationResultStatus.Forbidden, Is.True);
            Assert.That(status1 == status2, Is.True);
            Assert.That(status1 == status3, Is.True);
            Assert.That(status2 == status3, Is.True);
        }

        [Test]
        public void UnavailableStatusWithDifferentErrorsAreConsideredEqual()
        {
            //Arrange
            var status1 = ApiValidationResultStatus.Unavailable;
            var status2 = ApiValidationResultStatus.Unavailable with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.Unavailable with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == ApiValidationResultStatus.Unavailable, Is.True);
            Assert.That(status2 == ApiValidationResultStatus.Unavailable, Is.True);
            Assert.That(status3 == ApiValidationResultStatus.Unavailable, Is.True);
            Assert.That(status1 == status2, Is.True);
            Assert.That(status1 == status3, Is.True);
            Assert.That(status2 == status3, Is.True);
        }

        [Test]
        public void TooManyRequestsStatusWithDifferentErrorsAreConsideredEqual()
        {
            //Arrange
            var status1 = ApiValidationResultStatus.TooManyRequests;
            var status2 = ApiValidationResultStatus.TooManyRequests with { RetryAfter = TimeSpan.FromSeconds(100) };
            var status3 = ApiValidationResultStatus.TooManyRequests with { RetryAfter = TimeSpan.FromSeconds(500) };

            //Act Assert
            Assert.That(status1 == ApiValidationResultStatus.TooManyRequests, Is.True);
            Assert.That(status2 == ApiValidationResultStatus.TooManyRequests, Is.True);
            Assert.That(status3 == ApiValidationResultStatus.TooManyRequests, Is.True);
            Assert.That(status1 == status2, Is.True);
            Assert.That(status1 == status3, Is.True);
            Assert.That(status2 == status3, Is.True);
        }

        [Test]
        public void CanRoundTrip_ValidResultWithNoData()
        {
            var result = ApiValidationResult.Valid();
            var json = result.ToJsonWithNoTypeNameHandling();

            //Act
            var roundtripped = json.FromJson<ApiValidationResult>()!;

            //Assert
            Assert.That(result.Status, Is.EqualTo(ApiValidationResultStatus.Valid));
            Assert.That(roundtripped.Status, Is.EqualTo(ApiValidationResultStatus.Valid));
        }

        [Test]
        public void CanRoundTrip_ValidResultWithData()
        {
            var result = ApiValidationResult.Valid(new ResultData { Value1 = "123" });
            var json = result.ToJsonWithNoTypeNameHandling();

            //Act
            var roundtripped = json.FromJson<ApiValidationResult<ResultData>>();

            //Assert
            Assert.That(result!.Data!.Value1, Is.EqualTo("123"));
            Assert.That(result.Status, Is.EqualTo(ApiValidationResultStatus.Valid));
            Assert.That(roundtripped!.Data!.Value1, Is.EqualTo("123"));
            Assert.That(roundtripped!.Status, Is.EqualTo(ApiValidationResultStatus.Valid));
        }

        [TestCaseSource(nameof(GetAllStatusesThatSupportErrors))]
        public void CanRoundTrip_ResultWithValidationErrors_Status(ApiValidationResultStatus status)
        {
            var validationErrors = new List<ApiValidationError> { new ApiValidationError { Code = "1" } };

            status = status switch
            {
                ApiValidationResultStatus.ForbiddenStatus s => s with { ValidationErrors = validationErrors },
                ApiValidationResultStatus.InvalidStatus s => s with { ValidationErrors = validationErrors },
                ApiValidationResultStatus.NotFoundStatus s => s with { ValidationErrors = validationErrors },
                ApiValidationResultStatus.UnavailableStatus s => s with { ValidationErrors = validationErrors },
                _ => throw new NotImplementedException("{status}")
            };

            var result = new ApiValidationResult(status);
            var json = result.ToJsonWithNoTypeNameHandling();

            //Act
            var roundtripped = json.FromJson<ApiValidationResult>()!;

            //Assert
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.HasErrors, Is.EqualTo(true));
            Assert.That(result.ValidationErrors[0].Code, Is.EqualTo("1"));
            Assert.That(roundtripped.Status, Is.EqualTo(status));
            Assert.That(roundtripped.HasErrors, Is.EqualTo(true));
            Assert.That(roundtripped.ValidationErrors[0].Code, Is.EqualTo("1"));
        }


        public static IEnumerable<ApiValidationResultStatus> GetAllStatuses() => ApiValidationResultStatus.GetAllStatuses();

        public static IEnumerable<ApiValidationResultStatus> GetAllStatusesThatSupportErrors() => ApiValidationResultStatus.GetAllStatuses().Where(s => s is IApiValidationResultStatusErrors);

        [TestCaseSource(nameof(GetAllStatuses))]
        public void ValidStatusIsNotEqualToTheOtherStatus(ApiValidationResultStatus status)
        {
            if (status == ApiValidationResultStatus.Valid)
            {
                return;
            }

            //Arrange
            var valid1 = ApiValidationResultStatus.Valid;
            var valid2 = ApiValidationResultStatus.Valid with { Data = new { result = 1 } };
            var valid3 = ApiValidationResultStatus.Valid with { Data = "Hello" };


            //Act Assert
            Assert.That(valid1 == status, Is.False);
            Assert.That(valid2 == status, Is.False);
            Assert.That(valid3 == status, Is.False);
        }

        [TestCaseSource(nameof(GetAllStatuses))]
        public void NotFoundStatusIsNotEqualToTheOtherStatus(ApiValidationResultStatus status)
        {
            if (status == ApiValidationResultStatus.NotFound)
            {
                return;
            }

            //Arrange
            var status1 = ApiValidationResultStatus.NotFound;
            var status2 = ApiValidationResultStatus.NotFound with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.NotFound with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };


            //Act Assert
            Assert.That(status1 == status, Is.False);
            Assert.That(status2 == status, Is.False);
            Assert.That(status3 == status, Is.False);
        }

        [TestCaseSource(nameof(GetAllStatuses))]
        public void InvalidStatusIsNotEqualToTheOtherStatus(ApiValidationResultStatus status)
        {
            if (status == ApiValidationResultStatus.Invalid)
            {
                return;
            }

            //Arrange
            var status1 = ApiValidationResultStatus.Invalid;
            var status2 = ApiValidationResultStatus.Invalid with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.Invalid with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == status, Is.False);
            Assert.That(status2 == status, Is.False);
            Assert.That(status3 == status, Is.False);
        }

        [TestCaseSource(nameof(GetAllStatuses))]
        public void ForbiddenStatusIsNotEqualToTheOtherStatus(ApiValidationResultStatus status)
        {
            if (status == ApiValidationResultStatus.Forbidden)
            {
                return;
            }

            //Arrange
            var status1 = ApiValidationResultStatus.Forbidden;
            var status2 = ApiValidationResultStatus.Forbidden with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.Forbidden with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == status, Is.False);
            Assert.That(status2 == status, Is.False);
            Assert.That(status3 == status, Is.False);
        }

        [TestCaseSource(nameof(GetAllStatuses))]
        public void UnavailableStatusIsNotEqualToTheOtherStatus(ApiValidationResultStatus status)
        {
            if (status == ApiValidationResultStatus.Unavailable)
            {
                return;
            }

            //Arrange
            var status1 = ApiValidationResultStatus.Unavailable;
            var status2 = ApiValidationResultStatus.Unavailable with { ValidationErrors = new() { new() { Message = "1" } } };
            var status3 = ApiValidationResultStatus.Unavailable with { ValidationErrors = new() { new() { Message = "2" }, new() { Message = "3" } } };

            //Act Assert
            Assert.That(status1 == status, Is.False);
            Assert.That(status2 == status, Is.False);
            Assert.That(status3 == status, Is.False);
        }


        [TestCaseSource(nameof(GetAllStatuses))]
        public void TooManyRequestsStatusIsNotEqualToTheOtherStatus(ApiValidationResultStatus status)
        {
            if (status == ApiValidationResultStatus.TooManyRequests)
            {
                return;
            }

            //Arrange
            var status1 = ApiValidationResultStatus.TooManyRequests;
            var status2 = ApiValidationResultStatus.TooManyRequests with { RetryAfter = TimeSpan.FromSeconds(100) };
            var status3 = ApiValidationResultStatus.TooManyRequests with { RetryAfter = TimeSpan.FromSeconds(500) };

            //Act Assert
            Assert.That(status1 == status, Is.False);
            Assert.That(status2 == status, Is.False);
            Assert.That(status3 == status, Is.False);
        }

        public class ResultData
        {
            public string Value1 { get; set; } = string.Empty;
        }
    }
}
