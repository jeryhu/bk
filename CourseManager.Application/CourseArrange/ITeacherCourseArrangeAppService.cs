﻿using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CourseManager.CourseArrange.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseManager.CourseArrange
{
    public interface ITeacherCourseArrangeAppService : IApplicationService
    {
        Task TeacherArrangeCourse(CreateTeacherCourseArrangeInput input);
        TeacherCourseArrange GetArranage(TeacherCourseArrangeInput input);

        Task<ListResultDto<TeacherCourseArrangeListDto>> GetArranagesAsync();
        ListResultDto<TeacherCourseArrangeListDto> GetArranages();

        PagedResultDto<TeacherCourseArrangeListDto> GetPagedArranges(TeacherCourseArrangeInput input);
    }
}