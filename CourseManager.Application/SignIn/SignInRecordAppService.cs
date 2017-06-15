﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CourseManager.SignIn.Dto;
using Abp.Domain.Repositories;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.AutoMapper;
using CourseManager.Common;
using CourseManager.Category;
using CourseManager.Category.Dtos;
using CourseManager.Common.Enums;
using CourseManager.Students;
using CourseManager.Students.Dto;

namespace CourseManager.SignIn
{
    public class SignInRecordAppService : CourseManagerAppServiceBase, ISignInRecordAppService
    {

        private readonly IRepository<SignInRecord, string> _signInRepository;
        private readonly ICategorysAppService _categorysAppService;
        private readonly IStudentAppService _studentAppService;
        public SignInRecordAppService(
            IRepository<SignInRecord, string> signInRepository,
            ICategorysAppService categorysAppService,
             IStudentAppService studentAppService)
        {
            this._signInRepository = signInRepository;
            this._categorysAppService = categorysAppService;
            this._studentAppService = studentAppService;
        }

        private IQueryable<SignInRecord> GetSignInRecordByCondition(SignInInput input)
        {
            var query = _signInRepository.GetAll()
                        .WhereIf(!input.Id.IsNullOrEmpty(), o => o.Id == input.Id)
                        .WhereIf(input.BeginTime != null, o => o.BeginTime > input.BeginTime)
                        .WhereIf(input.BeginTime != null && input.EndTime != null, o => (input.BeginTime < o.BeginTime && o.EndTime < input.EndTime))
                        .WhereIf(!input.Filter.IsNullOrEmpty(), t => t.Type == input.Filter)
                        .Where(o => o.IsDeleted == false);
            query = string.IsNullOrEmpty(input.Sorting)
                        ? query.OrderByDescending(t => t.CreationTime)
                        : query.OrderBy(t => input.Sorting);
            return query;
        }
        public async Task CreateSignInRecord(CreateSignInInput input)
        {
            var record = input.MapTo<SignInRecord>();
            if (!string.IsNullOrEmpty(input.Id)) await _signInRepository.UpdateAsync(record);
            else
            {
                record.Id = IdentityCreator.NewGuid;
                record.CreatorUserId = AbpSession.UserId.Value;
                record.TeacherId = AbpSession.UserId.Value.ToString();
                TimeSpan ts1 = new TimeSpan(input.EndTime.Ticks);
                TimeSpan ts2 = new TimeSpan(input.BeginTime.Ticks);
                TimeSpan diff = ts1.Subtract(ts2).Duration();
                record.Duration = Convert.ToDecimal(Math.Ceiling(diff.TotalMinutes));
                var students = input.StudentId.Trim().Split(',');
                var studentIds = "";
                foreach (var stu in students)
                {
                    studentIds += _studentAppService.GetStudent(new StudentInput() { CnName = stu }).Id + ",";
                }
                record.StudentId = studentIds.TrimEnd(',');//孙京儿测试用 "fd3fd836655e4502a40db5acfca5d115" 自己录入数据的时候 输入准确学生名去匹配 如果是班级课就用逗号隔开
                await _signInRepository.InsertAsync(record);
            }
        }

        public PagedResultDto<SignInListDto> GetPagedSignInRecords(SignInInput input)
        {
            var query = GetSignInRecordByCondition(input);
            var count = query.Count();
            input.SkipCount = ((input.PIndex ?? 1) - 1) * (input.PSize ?? 10);
            input.MaxResultCount = input.PSize ?? 10;
            input.SkipCount = input.SkipCount < 0 ? 0 : input.SkipCount;
            var list = query.PageBy(input).ToList();
            var mapData = list.MapTo<List<SignInListDto>>();
            SetOtherExtendData(mapData);
            return new PagedResultDto<SignInListDto>(count, mapData); //list.MapTo<List<SignInListDto>>()
        }

        public SignInRecord GetSignIn(SignInInput input)
        {
            return _signInRepository.Get(input.Id);
        }

        public ListResultDto<SignInListDto> GetSignInRecords(SignInInput input)
        {
            var stus = GetSignInRecordByCondition(input);
            if (stus == null) return new ListResultDto<SignInListDto>();
            var list = stus.MapTo<List<SignInListDto>>();
            SetOtherExtendData(list);
            return new ListResultDto<SignInListDto>(list);
        }
        private void SetOtherExtendData(IEnumerable<SignInListDto> list)
        {
            if (list != null && list.Any())
            {
                //
                var categoryData = _categorysAppService.GetCategorysPageListBy(new CategorysInput { });
                var students = _studentAppService.GetStudents();
                StudentListDto studentModel = new StudentListDto();
                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item.ClassType) && categoryData.Any(c => c.Id == item.ClassType))
                        item.ClassTypeName = categoryData.FirstOrDefault(c => c.Id == item.ClassType).CategoryName;
                    if (!string.IsNullOrEmpty(item.Type) && categoryData.Any(c => c.Id == item.Type))
                        item.TypeName = categoryData.FirstOrDefault(c => c.Id == item.Type).CategoryName;
                    if (!string.IsNullOrEmpty(item.CourseType) && categoryData.Any(c => c.Id == item.CourseType))
                        item.CourseTypeName = categoryData.FirstOrDefault(c => c.Id == item.CourseType).CategoryName;
                    if (!string.IsNullOrEmpty(item.UnNormalType) && categoryData.Any(c => c.Id == item.UnNormalType))
                        item.UnNormalTypeName = categoryData.FirstOrDefault(c => c.Id == item.UnNormalType).CategoryName;
                    if (!string.IsNullOrEmpty(item.CourseAddressType) && categoryData.Any(c => c.Id == item.CourseAddressType))
                        item.CourseAddressTypeName = categoryData.FirstOrDefault(c => c.Id == item.CourseAddressType).CategoryName;
                    var studentIds = item.StudentId.Split(',');
                    string stuName = string.Empty;
                    foreach (var stu in studentIds)
                    {
                        studentModel = students.Items.FirstOrDefault(s => s.Id == stu);
                        if (studentModel != null && !string.IsNullOrEmpty(studentModel.Id))
                            stuName += students.Items.FirstOrDefault(s => s.Id == stu).CnName + ",";
                    }
                    item.StudentName = stuName.TrimEnd(',');
                }
            }
        }

        public async Task<ListResultDto<SignInListDto>> GetSignInRecordsAsync()
        {
            var stus = await _signInRepository.GetAllListAsync();

            return new ListResultDto<SignInListDto>(
                stus.MapTo<List<SignInListDto>>()
                );
        }
    }
}
