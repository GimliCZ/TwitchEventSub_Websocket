using Twitch.EventSub.API.Models;

namespace Twitch.EventSub.API.Extensions
{
    public static class CreateSubscriptionRequestExtension
    {
        private static readonly Dictionary<SubscriptionType, (string Type, string Version, List<ConditionType> Conditions)> TypeVersionConditionMap = GenerateSubscriptionDisctionary();

        private static Dictionary<SubscriptionType, (string Type, string Version, List<ConditionType> Conditions)> GenerateSubscriptionDisctionary()
        {
            var newDict = new Dictionary<SubscriptionType, (string Type, string Version, List<ConditionType> Conditions)>();

            foreach (var register in Twitch.EventSub.SubsRegister.Register.GetRegisterList())
            {
                newDict.Add(register.SubscriptionType, (register.Key, register.Ver, register.Conditions));
            }
            return newDict;
        }

        private static List<ConditionType> CondList(params ConditionType[] types)
        {
            var list = new List<ConditionType>();
            list.AddRange(types);
            return list;
        }

        //Reward Id enables to sub to specific reward only. As null it subs all rewards
        public static CreateSubscriptionRequest SetSubscriptionType(this CreateSubscriptionRequest request,
         SubscriptionType subscriptionType, string userId, string? rewardId = null)
        {
            if (TypeVersionConditionMap.TryGetValue(subscriptionType, out var typeVersionCondition))
            {
                request.Type = typeVersionCondition.Type;
                request.Version = typeVersionCondition.Version;
                foreach (var conditionType in typeVersionCondition.Conditions)
                {
                    switch (conditionType)
                    {
                        case ConditionType.BroadcasterUserId:
                            request.Condition.BroadcasterUserId = userId;
                            break;

                        case ConditionType.ToBroadcasterUserId:
                            request.Condition.ToBroadcasterUserId = userId;
                            break;

                        case ConditionType.ModeratorUserId:
                            request.Condition.ModeratorUserId = userId;
                            break;
                        //webhook only
                        /*    case ConditionType.OrganizationId:
                                request.Condition.OrganizationId = organizationId;
                                break;

                            case ConditionType.CampaignId:
                                request.Condition.CampaignId = campaignId;
                                break;

                            case ConditionType.CategoryId:
                                request.Condition.CategoryId = categoryId;
                                break;*/
                        case ConditionType.ClientId:
                            request.Condition.ClientId = userId;
                            break;
                        /*case ConditionType.ExtensionClientId:
                            request.Condition.ExtensionClientId = userId;
                            break;*/
                        case ConditionType.UserId:
                            request.Condition.UserId = userId;
                            break;

                        case ConditionType.RewardId:
                            request.Condition.RewardId = rewardId;
                            break;

                        default:
                            throw new ArgumentException("Invalid subscription type");
                    }
                }

                return request;
            }
            throw new ArgumentException("Invalid subscription");
        }
    }
}