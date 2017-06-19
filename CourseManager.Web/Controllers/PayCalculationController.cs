﻿using Abp.Web.Mvc.Authorization;
using CourseManager.ClassHourStatistics;
using CourseManager.ClassHourStatistics.Dto;
using CourseManager.Core.EntitiesFromCustom;
using CourseManager.CourseArrange;
using CourseManager.CourseArrange.Dto;
using CourseManager.Web.Models.ClassHourStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseManager.Web.Models.PayCalculation;
namespace CourseManager.Web.Controllers
{
    [AbpMvcAuthorize]
    public class PayCalculationController : CourseManagerControllerBase
    {
        private readonly IClassHourStatisticsAppService _teacherClassHoursStatisticsAppService;
        public PayCalculationController(IClassHourStatisticsAppService teacherClassHoursStatisticsAppService)
        {
            this._teacherClassHoursStatisticsAppService = teacherClassHoursStatisticsAppService;
        }

        public ActionResult PayCalculation()
        {
            ViewBag.ActiveMenu = "PayCalculation";
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult GetPayCalculation(ClassHourStatisticsInput input)
        {
            var result = _teacherClassHoursStatisticsAppService.GetClassHourStatistics(input).Items;
            /*说明：
             --续学人数 通过group统计 某个学生上课总时长 是否达到15
            --基本工资　通过计算总课时　是否达到70　如果达到了　其他的一些福利　再进行计算，如果在40-７０之间就只是拿到底薪4k
            --早课和晚课通过 上课开始时间是否小于早上七点半 和开始时间是否大于晚上七点 进行统计
            --1对1课时数和班级数 通过课程类型来统计
            --外派次数 通过上课类型来统计
            --统计输出的话 就是按照自定义的格式综合输出
             */
            var classResult = result.Where(r => r.ClassType == "f756be8fe8b6487dbb50e6d63c69895c").ToList();
            var one2OneResult = result.Where(r => r.ClassType == "64c951d110044a51bd83c7e7e82f96ec").ToList();
            List<decimal> durations = new List<decimal>();
            var totalDuration = decimal.Round(result.Sum(r => r.Duration) / 60, 1);
            var one2oneDuration = one2OneResult.Sum(r => r.Duration);
            var classDuration = classResult.Sum(r => r.Duration);
            var beginTimeDay = input.BeginTime.Day;
            var endTimeDay = input.EndTime.Day;
            decimal[] classHoursArray = new decimal[endTimeDay - beginTimeDay + 1]; //取得的值是大于等于开始时间 小于结束时间
            decimal[] one2OneDurationsArray = new decimal[endTimeDay - beginTimeDay + 1];
            decimal[] classCourseDurationsArray = new decimal[endTimeDay - beginTimeDay + 1];
            PayCalculationViewModel vm = new PayCalculationViewModel();
            var totalOfficeHours = 0.0M;
            for (int i = beginTimeDay - 1; i < endTimeDay; i++)
            {
                var duration = 0.0M;
                classHoursArray[i] = duration;
                if (result.Any(v => v.EndTime.Day == i + 1))
                {
                    duration = decimal.Round(result.Where(r => r.EndTime.Day == i + 1).Sum(c => c.Duration) / 60, 1);
                    classHoursArray[i] = duration;
                }
                if (one2OneResult.Any(v => v.EndTime.Day == i + 1))
                {
                    one2OneDurationsArray[i] = decimal.Round(one2OneResult.Where(r => r.EndTime.Day == i + 1).Sum(c => c.Duration) / 60, 1);
                }
                if (classResult.Any(v => v.EndTime.Day == i + 1))
                {
                    classCourseDurationsArray[i] = decimal.Round(classResult.Where(r => r.EndTime.Day == i + 1).Sum(c => c.Duration) / 60, 1);
                }
                var totalMinutes = 0.0;
                if (result.Any(r => r.EndTime.Day == i + 1))
                {
                    var maxEndTime = result.Where(r => r.EndTime.Day == i + 1).Max(t => t.EndTime);
                    var minBeginTime = result.Where(r => r.EndTime.Day == i + 1).Min(t => t.BeginTime);
                    totalMinutes = (maxEndTime - minBeginTime).TotalMinutes;
                }
                totalOfficeHours += decimal.Round(Convert.ToDecimal(totalMinutes) / 60, 1);
            }
            vm.TeacherName = string.IsNullOrEmpty(input.TeacherName) ? "胡盼" : input.TeacherName;
            decimal basicSalary = CourseManagerConsts.BasicSalary;
            //底薪说明：20小时以下2000元，20小时～40小时3000元，40以上4000元
            if (totalDuration < CourseManagerConsts.BasicSalaryHours)
            {
                if (totalDuration < 20) basicSalary = 2000;
                else if (totalDuration > 20 && totalDuration < 40) basicSalary = 3000;
            }
            vm.BasicSalary = basicSalary;
            vm.One2OneDuration = decimal.Round(one2oneDuration / 60, 1);
            vm.ClassDuration = decimal.Round(classDuration / 60, 1);
            vm.TotalDuration = totalDuration;
            vm.EarlyCourseTimes = result.Where(r => r.BeginTime.Hour <= CourseManagerConsts.EarlyHour).Count();
            vm.NightCourseTimes = result.Where(r => r.BeginTime.Hour >= CourseManagerConsts.NigthHour).Count();
            vm.AssignmentTimes = result.Where(r => r.CourseAddressType == "55c431fd26b845b0a00880243d1a25a3").Count(); //外派
            vm.OfficeHours = totalOfficeHours;
            vm.AllOfficeHoursBonus = totalOfficeHours >= CourseManagerConsts.JuneOfficeHours ? CourseManagerConsts.AllOfficeHoursBonus : 0.0M;
            ResultData data = new ResultData()
            {
                returnData = new Dictionary<string, object> {
                    { "result",vm}
                }
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}