function DragDropFieldsControl(id, parentObject, data, availableFieldsListName, usedFieldsListName, type) {
    this.Data = data;
    this.Element = document.getElementById(id);
    this.ParentObject = parentObject;
	this.AvailableFieldsContainer = null;
	this.UsedFieldsContainer = null;
    this.AvailableFieldsCount = 0;
	this.UsedFieldsCount = 0;
    this.FieldTypesCount = new Array();
    this.SelectedType = 'All Fields';

	this.AvailableFieldsListName = availableFieldsListName;
	this.UsedFieldsListName = usedFieldsListName;

	this.AvailableFieldsCountElement = null;
	this.UsedFieldsCountElement = null;
	this.AvailableFieldsContainerArray = new Array();

    this.IsFieldEdit = false;
	this.Type = type;
    this.FieldTypes = new Array();
    this.ChangeDefaultName();
};

DragDropFieldsControl.prototype.ChangeDefaultName = function () {
    for (var idx in this.Data) {
        var item = this.Data[idx];
        if (item) {
            item.DefaultName = item.Name;
        }
    }
};

DragDropFieldsControl.prototype.FillFieldTypes = function () {

    if (this.Data && this.Data[0] && this.Data[0].Category != '') {
        this.FieldTypes.push('All Fields');
        this.FieldTypesCount['All Fields'] = 0;
    }

    for (var idx in this.Data) {


        if (this.FieldTypesCount[this.Data[idx].Category] && !this.Data[idx].IsUsed) this.FieldTypesCount[this.Data[idx].Category]++;
        else this.FieldTypesCount[this.Data[idx].Category] = 1;

        var exists = false;
        for (var type in this.FieldTypes) {
            if (this.Data[idx].Category == this.FieldTypes[type]) {
                exists = true;
                break;
            }
        }

        if (!exists) this.FieldTypes.push(this.Data[idx].Category);
    }
};


DragDropFieldsControl.prototype.Render = function () {
    var parentObject = this;

    if (this.Element.innerHTML == '') {
        var attributes = {
            'usedFieldsContainerId': this.getClientID('usedFieldsContainerId'),
            'availableFieldsContainerId': this.getClientID('availableFieldsContainerId'),
            'availableFieldsListName': this.AvailableFieldsListName,
            'usedFieldsListName': this.UsedFieldsListName,
            'availableFieldsCountElement': this.getClientID('availableFieldsCountElement'),
            'usedFieldsCountElement': this.getClientID('usedFieldsCountElement'),
            'centerContainerId': this.getClientID('centerContainerId'),
            'typesContainer': this.getClientID('typesContainer')
        };

        $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_DragDropContainer'], attributes));

        if (this.FieldTypes.length == 0 && this.Data && this.Data[0] && this.Data[0].Category != '')
            this.FillFieldTypes();


        this.AvailableFieldsCountElement = $('#' + this.getClientID('availableFieldsCountElement'))[0];
        this.UsedFieldsCountElement = $('#' + this.getClientID('usedFieldsCountElement'))[0];

        this.AvailableFieldsContainer = document.getElementById(this.getClientID('availableFieldsContainerId'));
        this.UsedFieldsContainer = document.getElementById(this.getClientID('usedFieldsContainerId'));

        if (this.Type == 'layout') {
            var typesContainer = $('#' + this.getClientID('typesContainer'));

            for (var type in this.FieldTypes) {
                var attr = {
                    'groupItemHeader': (this.getClientID('groupItemHeader') + this.FieldTypes[type]).replace(/ /g, '_'),
                    'groupItemListName': this.FieldTypes[type],
                    'groupItemHeaderCount': (this.getClientID('groupItemHeaderCount') + this.FieldTypes[type]).replace(/ /g, '_'),
                    'category': this.FieldTypes[type]
                };


                $(typesContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_groupItemHeader'], attr));
                if (type == '0') {
                    $('div.close_group_column', typesContainer).attr('class', 'open_group_column');

                }
                $(("#" + this.getClientID('groupItemHeader') + this.FieldTypes[type]).replace(/ /g, '_')).click(function () {
                    parentObject.SelectedType = this.getAttribute('category');
                    $('div.open_group_column').each(function () {
                        this.className = 'close_group_column';
                    });
                    $('div.close_group_column', this).attr('class', 'open_group_column');
                    $(this).after(parentObject.AvailableFieldsContainer);
                    parentObject.Render();
                });

            }

            $(("#" + this.getClientID('groupItemHeader') + this.SelectedType).replace(/ /g, '_')).after(this.AvailableFieldsContainer);
        }
    }



    if (this.Type == 'layout') {
        for (var idx in this.Data) {
            if (this.Data[idx].DefaultName != this.Data[idx].Name) {
                this.Data[idx].Name = this.Data[idx].DefaultName;
            }
        }

        this.AvailableFieldsContainer.innerHTML = '';
        this.UsedFieldsContainer.innerHTML = '';

    } else {
        //        $(this.Element).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_DragDropContainer'], attributes));
        //
        //        this.AvailableFieldsCountElement = $('#' + this.getClientID('availableFieldsCountElement'))[0];
        //        this.UsedFieldsCountElement = $('#' + this.getClientID('usedFieldsCountElement'))[0];
        //
        //        this.AvailableFieldsContainer = document.getElementById(this.getClientID('availableFieldsContainerId'));
        //        this.UsedFieldsContainer = document.getElementById(this.getClientID('usedFieldsContainerId'));
    }



    for (var i in this.Data) {
        var att = {
            'itemId': this.getClientID(this.Data[i].ID),
            'fieldId': this.getClientID(this.Data[i].ID),
            'fieldName': this.Data[i].Name,
            'editFieldNameBtn': 'editBtn_' + this.getClientID(this.Data[i].ID),
            'ascendingFieldBtn': 'ascendingFieldBtn_' + this.getClientID(this.Data[i].ID),
            'descendingFieldBtn': 'descendingFieldBtn_' + this.getClientID(this.Data[i].ID),
            'saveFieldName': 'saveFieldName_' + this.getClientID(this.Data[i].ID),
            'editTextBox': 'editTextBox_' + this.getClientID(this.Data[i].ID),
            'class': "",
            'ascendingClass': "",
            'descendingClass': "",
            'category': this.Data[i].Category
        };

        switch (this.Type) {
            case 'sorting':
                if (this.Data[i].SortOrder > -1) {
                    att["class"] = 'liItemNormalSort';
                    if (this.Data[i].SortDirection == 1) {
                        att["ascendingClass"] = 'black_left_sort';
                        att["descendingClass"] = 'gray_right_sort';
                    } else {
                        att["ascendingClass"] = 'gray_left_sort';
                        att["descendingClass"] = 'black_right_sort';
                    }
                    $(this.UsedFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));


                } else {
                    att["class"] = 'liItemNormalAv';
                    att["ascendingClass"] = 'black_left_sort';
                    att["descendingClass"] = 'gray_right_sort';
                    $(this.AvailableFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));

                }

                $('#' + 'ascendingFieldBtn_' + this.getClientID(this.Data[i].ID)).click(function () {
                    var id = this.id.split('_')[2];
                    if (this.className == 'gray_left_sort') {
                        this.className = 'black_left_sort';
                        $('#' + 'descendingFieldBtn_' + parentObject.getClientID(id))[0].className = 'gray_right_sort';
                        parentObject.UpdateFieldById(parentObject.Data, id, 'SortDirection', 1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, id, 'SortDirection', 1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, id, 'SortDirection', 1);
                    }
                });
                $('#' + 'descendingFieldBtn_' + this.getClientID(this.Data[i].ID)).click(function () {
                    var id = this.id.split('_')[2];
                    if (this.className == 'gray_right_sort') {
                        this.className = 'black_right_sort';
                        $('#' + 'ascendingFieldBtn_' + parentObject.getClientID(id))[0].className = 'gray_left_sort';
                        parentObject.UpdateFieldById(parentObject.Data, id, 'SortDirection', 2);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, id, 'SortDirection', 2);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, id, 'SortDirection', 2);
                    }
                });
                break;
            case 'layout':
                if (this.Data[i].IsUsed) {
                    att["class"] = 'liItemNormal';
                    $(this.UsedFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));
                } else {
                    att["class"] = 'liItemNormalAv';

                    if (this.SelectedType == 'All Fields')
                        $(this.AvailableFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));
                    else if (this.SelectedType == this.Data[i].Category)
                        $(this.AvailableFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));
                }

                $('#' + 'editBtn_' + this.getClientID(this.Data[i].ID)).click(function () {
                    if (parentObject.IsFieldEdit) return;

                    parentObject.IsFieldEdit = true;

                    var li = $(this).parents('li')[0];
                    li.className = 'liItemChanged';
                    $('input.change_field_name', li)[0].focus();
                    $('input.change_field_name', li).select();
                });


                $('#' + 'saveFieldName_' + this.getClientID(this.Data[i].ID)).click(function () {
                    var li = $(this).parents('li')[0];
                    if (parentObject.Type == 'sorting') {
                        li.className = 'liItemNormalSort';
                    } else {
                        li.className = 'liItemNormal';
                    }


                    $('div.field_name', li).html($('input.change_field_name', li).val());
                    var id = this.id.split('_')[2];
                    var newName = $('input.change_field_name', li).val();
                    parentObject.UpdateFieldById(parentObject.Data, id, 'Name', newName);
                    parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, id, 'Name', newName);

                    parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.Data.ReportFields, id, 'Name', newName);
                    parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, id, 'Name', newName);
                    parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.dragDropControl.Data, id, 'Name', newName);
                    parentObject.ParentObject.GoupingTab.dragDropControl.Render();
                    parentObject.ParentObject.GoupingTab.SummarizeControl.Data = parentObject.ParentObject.GoupingTab.GetSummarizeFields();
                    parentObject.ParentObject.GoupingTab.SummarizeControl.Render();

                    parentObject.UpdateFieldById(parentObject.ParentObject.SortingTab.Data.ReportFields, id, 'Name', newName);
                    parentObject.UpdateFieldById(parentObject.ParentObject.SortingTab.dragDropControl.Data, id, 'Name', newName);
                    $('.field_name', $('#' + parentObject.ParentObject.SortingTab.dragDropControl.Element.id + '_' + id + '_sorting')[0]).html(newName);
                    parentObject.IsFieldEdit = false;
                });

                $('#' + 'editTextBox_' + this.getClientID(this.Data[i].ID)).keyup(function () {
                    if (arguments && arguments.length > 0 && arguments[0].keyCode == 13) {
                        var li = $(this).parents('li')[0];
                        if (parentObject.Type == 'sorting') {
                            li.className = 'liItemNormalSort';
                        } else {
                            li.className = 'liItemNormal';
                        }
                        $('div.field_name', li).html($('input.change_field_name', li).val());
                        var id = this.id.split('_')[2];
                        var newName = $('input.change_field_name', li).val();
                        parentObject.UpdateFieldById(parentObject.Data, id, 'Name', newName);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, id, 'Name', newName);

                        parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.Data.ReportFields, id, 'Name', newName);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, id, 'Name', newName);
                        parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.dragDropControl.Data, id, 'Name', newName);
                        parentObject.ParentObject.GoupingTab.dragDropControl.Render();
                        parentObject.ParentObject.GoupingTab.SummarizeControl.Data = parentObject.ParentObject.GoupingTab.GetSummarizeFields();
                        parentObject.ParentObject.GoupingTab.SummarizeControl.Render();

                        parentObject.UpdateFieldById(parentObject.ParentObject.SortingTab.Data.ReportFields, id, 'Name', newName);
                        parentObject.UpdateFieldById(parentObject.ParentObject.SortingTab.dragDropControl.Data, id, 'Name', newName);
                        $('.field_name', $('#' + parentObject.ParentObject.SortingTab.dragDropControl.Element.id + '_' + id + '_sorting')[0]).html(newName);
                        parentObject.IsFieldEdit = false;
                    }
                });

                break;
            case 'grouping':
                att["class"] = 'liItemNormalAv';
                if (this.Data[i].GroupOrder > -1) {
                    $(this.UsedFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));
                } else {
                    $(this.AvailableFieldsContainer).append(TemplateEngine.Format(TemplateManager.templates['DragDropFields_fieldItem'], att));

                }

                break;
        }




    };


    $('li', this.Element).dblclick(function () {
        if (this.parentNode.id == parentObject.AvailableFieldsContainer.id) {
            $(this).appendTo('#' + parentObject.UsedFieldsContainer.id);
            if (parentObject.Type == 'sorting') {
                this.className = 'liItemNormalSort';
            } else if (parentObject.Type == 'layout') {
                this.className = 'liItemNormal';
            }

            if (parentObject.Type == 'layout') {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'IsUsed', true);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'IsUsed', true);
                parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.dragDropControl.Data, this.id.split('_')[1], 'IsUsed', true);
                parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.Data.ReportFields, this.id.split('_')[1], 'IsUsed', true);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'IsUsed', true);

            }
        }
        else {
            $(this).appendTo('#' + parentObject.AvailableFieldsContainer.id);
            this.className = 'liItemNormalAv';

            if (parentObject.Type == 'layout') {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'IsUsed', false);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'IsUsed', false);
                parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.dragDropControl.Data, this.id.split('_')[1], 'IsUsed', false);
                parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.Data.ReportFields, this.id.split('_')[1], 'IsUsed', false);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'IsUsed', false);
            }
        }

        if (parentObject.Type == 'layout') {

            var idx = 1;
            $('li', parentObject.UsedFieldsContainer).each(function () {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'ColumnOrder', idx);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'ColumnOrder', idx);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'ColumnOrder', idx);
                idx++;
            });

            $('li', parentObject.AvailableFieldsContainer).each(function () {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'ColumnOrder', -1);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'ColumnOrder', -1);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'ColumnOrder', -1);
                idx++;
            });

            parentObject.ParentObject.GoupingTab.dragDropControl.Data = parentObject.ParentObject.GoupingTab.RefreshDragDropFields();
            parentObject.ParentObject.GoupingTab.dragDropControl.Render();

            parentObject.ParentObject.GoupingTab.SummarizeControl.Data = parentObject.ParentObject.GoupingTab.GetSummarizeFields();
            parentObject.ParentObject.GoupingTab.SummarizeControl.Render();
        }

        if (parentObject.Type == 'sorting') {

            var idx = 1;
            $('li', parentObject.UsedFieldsContainer).each(function () {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'SortOrder', idx);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'SortOrder', idx);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'SortOrder', idx);
                idx++;
            });

            $('li', parentObject.AvailableFieldsContainer).each(function () {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'SortOrder', -1);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'SortOrder', -1);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'SortOrder', -1);
                idx++;
            });

        }

        if (parentObject.Type == 'grouping') {
            var idx = 1;
            $('li', parentObject.UsedFieldsContainer).each(function () {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'GroupOrder', idx);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'GroupOrder', idx);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'GroupOrder', idx);
                idx++;
            });

            $('li', parentObject.AvailableFieldsContainer).each(function () {
                parentObject.UpdateFieldById(parentObject.Data, this.id.split('_')[1], 'GroupOrder', -1);
                parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'GroupOrder', -1);
                parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, this.id.split('_')[1], 'GroupOrder', -1);
                idx++;
            });


        }

        parentObject.SetFieldsCount();

        if (this.parentNode.id == parentObject.AvailableFieldsContainer.id) {
            parentObject.Data = parentObject.ParentObject.RefreshDragDropFields();
            parentObject.Render();
            RightTabHeight();
        }

    });


    $(this.UsedFieldsContainer).sortable({
        handle: '.drag_drop_item',
        connectWith: '#' + this.AvailableFieldsContainer.id,
        stop: function (event, ui) {
            parentObject.SetFieldsCount();
            if ($(ui.item[0]).parents('ul')[0].id == parentObject.AvailableFieldsContainer.id) {
                ui.item[0].className = 'liItemNormalAv';

                if (parentObject.Type == 'layout') {

                    parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'IsUsed', false);
                    parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.dragDropControl.Data, ui.item[0].id.split('_')[1], 'IsUsed', false);

                    parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'IsUsed', false);
                    parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.Data.ReportFields, ui.item[0].id.split('_')[1], 'IsUsed', false);
                    parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'IsUsed', false);

                    var idx = 1;
                    $('li', parentObject.UsedFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'ColumnOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', idx);
                        idx++;
                    });

                    $('li', parentObject.AvailableFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'ColumnOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', -1);
                        idx++;
                    });

                    parentObject.ParentObject.GoupingTab.dragDropControl.Data = parentObject.ParentObject.GoupingTab.RefreshDragDropFields();
                    parentObject.ParentObject.GoupingTab.dragDropControl.Render();

                    parentObject.ParentObject.GoupingTab.SummarizeControl.Data = parentObject.ParentObject.GoupingTab.GetSummarizeFields();
                    parentObject.ParentObject.GoupingTab.SummarizeControl.Render();
                }

                if (parentObject.Type == 'sorting') {

                    var idx = 1;
                    $('li', parentObject.UsedFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'SortOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', idx);
                        idx++;
                    });

                    $('li', parentObject.AvailableFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'SortOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', -1);
                        idx++;
                    });

                }

                if (parentObject.Type == 'grouping') {

                    var idx = 1;
                    $('li', parentObject.UsedFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'GroupOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', idx);
                        idx++;
                    });

                    $('li', parentObject.AvailableFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'GroupOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', -1);
                        idx++;
                    });


                }

                parentObject.Data = parentObject.ParentObject.RefreshDragDropFields();
                parentObject.Render();
                RightTabHeight();
            }
        }
    });

    $(this.AvailableFieldsContainer).sortable({
        handle: '.drag_drop_item',
        connectWith: '#' + this.UsedFieldsContainer.id,
        stop: function (event, ui) {
            parentObject.SetFieldsCount();

            if ($(ui.item[0]).parents('ul')[0].id == parentObject.UsedFieldsContainer.id) {
                if (parentObject.Type == 'sorting') {
                    ui.item[0].className = 'liItemNormalSort';
                } else if (parentObject.Type == 'layout') {
                    ui.item[0].className = 'liItemNormal';
                }

                if (parentObject.Type == 'layout') {

                    parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'IsUsed', true);
                    parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.dragDropControl.Data, ui.item[0].id.split('_')[1], 'IsUsed', true);

                    parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'IsUsed', true);
                    parentObject.UpdateFieldById(parentObject.ParentObject.GoupingTab.Data.ReportFields, ui.item[0].id.split('_')[1], 'IsUsed', true);
                    parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'IsUsed', true);

                    var idx = 1;
                    $('li', parentObject.UsedFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'ColumnOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', idx);
                        idx++;
                    });

                    $('li', parentObject.AvailableFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'ColumnOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'ColumnOrder', -1);
                        idx++;
                    });

                    parentObject.ParentObject.GoupingTab.dragDropControl.Data = parentObject.ParentObject.GoupingTab.RefreshDragDropFields();
                    parentObject.ParentObject.GoupingTab.dragDropControl.Render();

                    parentObject.ParentObject.GoupingTab.SummarizeControl.Data = parentObject.ParentObject.GoupingTab.GetSummarizeFields();
                    parentObject.ParentObject.GoupingTab.SummarizeControl.Render();
                }

                if (parentObject.Type == 'sorting') {

                    var idx = 1;
                    $('li', parentObject.UsedFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'SortOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', idx);
                        idx++;
                    });

                    $('li', parentObject.AvailableFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'SortOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'SortOrder', -1);
                        idx++;
                    });

                }

                if (parentObject.Type == 'grouping') {
                    var idx = 1;
                    $('li', parentObject.UsedFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'GroupOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', idx);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', idx);
                        idx++;
                    });

                    $('li', parentObject.AvailableFieldsContainer).each(function () {
                        parentObject.UpdateFieldById(parentObject.Data, ui.item[0].id.split('_')[1], 'GroupOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', -1);
                        parentObject.UpdateFieldById(parentObject.ParentObject.ParentObject.Data.ReportFields, ui.item[0].id.split('_')[1], 'GroupOrder', -1);
                        idx++;
                    });

                }
            }
        }
    });

    this.SetFieldsCount();
};

DragDropFieldsControl.prototype.GetUsedFields = function () {
    var resultArray = new Array();

    $('li', this.UsedFieldsContainer).each(function () {
        resultArray.push(this.getAttribute('fieldId'));
    });

    return resultArray;
};


DragDropFieldsControl.prototype.SetFieldsCount = function () {
    this.UsedFieldsCount = $('li', this.UsedFieldsContainer).length;
    this.AvailableFieldsCount = 0;
    for (var type in this.FieldTypesCount) {
        this.FieldTypesCount[type] = 0;
    }

    for (var idx in this.Data) {
        if (!this.Data[idx].IsUsed) {
            this.AvailableFieldsCount++;
            this.FieldTypesCount[this.Data[idx].Category]++;
        }
    }

    for (var type in this.FieldTypesCount) {
        if (type == 'All Fields') {
            $('#' + (this.getClientID('groupItemHeaderCount') + type).replace(/ /g, '_')).html('(' + this.AvailableFieldsCount + ')');
        } else {
            $('#' + (this.getClientID('groupItemHeaderCount') + type).replace(/ /g, '_')).html('(' + this.FieldTypesCount[type] + ')');
        }
    }

    $(this.AvailableFieldsCountElement).html('(' + this.AvailableFieldsCount + ')');
    $(this.UsedFieldsCountElement).html('(' + this.UsedFieldsCount + ')');

    $('li', this.UsedFieldsContainer).each(function () {
        $('.drag_drop_item', this).css('border-bottom', '1px solid #d4d4d4');
    });

    $('li', this.AvailableFieldsContainer).each(function () {
        $('.drag_drop_item', this).css('border-bottom', '1px solid #d4d4d4');
    });

    $('li:last-child', this.UsedFieldsContainer).each(function () {
        $('.drag_drop_item', this).css('border-bottom', '1px solid white');
    });

    $('li:last-child', this.AvailableFieldsContainer).each(function () {
        $('.drag_drop_item', this).css('border-bottom', '1px solid white');
    });
};

DragDropFieldsControl.prototype.UpdateFieldById = function (data, id, field, value) {
    for (var i in data) {
        if (data[i].ID == id) {
            data[i][field] = value;
        }
    }
};

DragDropFieldsControl.prototype.getClientID = function (name) {
    return this.Element.id + '_'+ name + '_' +  this.Type  ;
};

function RightTabHeight() {
    if($('div[category]').length > 0) {
         var heightCat = 0;
         $('div[category]').each(function () {
             var ddd = $(this).outerHeight();
             heightCat = heightCat + ddd;
         });
    }

    var heightMain = $(document.getElementById("MainLeft")).height();
    var heightUl = (heightMain - heightCat - 126) + 'px';
    var heightEl = ($(document.getElementById("MainLeft")).height() * 1 - 126) + "px";
    var heightElGrouping = ($(document.getElementById("MainLeft")).height() * 1 - 274) + "px";
    var heightElViewReport = ($(document.getElementById("MainLeft")).height() * 1 - 48) + "px";
    var heightElContentViewReport = ($(document.getElementById("MainLeft")).height() * 1 - 100) + "px";

    if (document.getElementById('columnsDragDropFields_centerContainerId_layout_left')) {
        try {
            document.getElementById('columnsDragDropFields_centerContainerId_layout_left').style.height = heightEl;
            document.getElementById('columnsDragDropFields_centerContainerId_layout_right').style.height = heightEl;
            document.getElementById('columnsDragDropFields_availableFieldsContainerId_layout').style.height = heightUl;
            document.getElementById('sortingDragDropFields_centerContainerId_sorting_left').style.height = heightEl;
            document.getElementById('sortingDragDropFields_centerContainerId_sorting_right').style.height = heightEl;
            document.getElementById('groupingDragDropFields_centerContainerId_grouping_left').style.height = heightElGrouping;
            document.getElementById('groupingDragDropFields_centerContainerId_grouping_right').style.height = heightElGrouping;
            document.getElementById('ViewReportTabContainer').style.height = heightElViewReport;
            document.getElementById('ViewReportTabContainer_content').style.height = heightElContentViewReport;
        } catch(e) {

        } 
        
    }
};
