namespace InputTemplates {

    interface DropDownValues {

        id: string;

        name: string;

    }

    export function generateInput(field: DynamicFormField, providerSection: number): JQuery {
        let $wrapper = $(`<div class="form-group"></div>`);
        let $field = null;

        switch (field.type) {
            case 'hidden':
                $field = generateHiddenInput(field, $wrapper, providerSection);
                break;

            case 'string':
                $field = generateStringInput(field, $wrapper, providerSection);
                break;

            case 'password':
                $field = generateStringInput(field, $wrapper, providerSection);
                $field.attr('type', 'password');
                break;

            case 'bool':
                $field = generateBooleanInput(field, $wrapper, providerSection);
                break;

            case 'dropdown':
                $field = generateDropdownInput(field, $wrapper, providerSection);
                break;

            case 'multi-select':
                $field = generateMultiSelectInput(field, $wrapper, providerSection);
                break;

            case 'cron':
                $field = generateCronInput(field, $wrapper, providerSection);
                break;
        }

        addValidationAttributes($field, field);

        return $wrapper;
    }

    function generateHiddenInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): JQuery {
        let fieldName = generateFieldName(field, providerSection);
        let value = field.value ? field.value : '';
        
        $wrapper.removeClass('form-group').attr('hidden', 'hidden');

        let $field = $(`<input type="hidden" name="${fieldName}" value="${value}" />`);
        $field.appendTo($wrapper);

        return $field;
    }

    function generateStringInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): JQuery {
        let fieldName = generateFieldName(field, providerSection);
        let value = field.value ? field.value : '';
        
        appendLabel($wrapper, field.name, fieldName);

        let $field = $(`<input class="form-control boxed" type="text" id="${fieldName}" name="${fieldName}" value="${value}" />`);
        $field.appendTo($wrapper);

        return $field;
    }

    function generateBooleanInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): JQuery {
        let fieldName = generateFieldName(field, providerSection);
        let checked = field.value && field.value.toLowerCase() === 'true' ? `checked="checked"` : '';

        $wrapper.removeClass('form-group');

        $wrapper.append(`<label><input class="checkbox" type="checkbox" id="${fieldName}" name="${fieldName}" value="true" ${checked} /><span>${field.name}</span></label>`);

        return $wrapper.find('input');
    }

    function generateDropdownInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): JQuery {
        let fieldName = generateFieldName(field, providerSection);
        
        appendLabel($wrapper, field.name, fieldName);

        let $list = $(`<select class="form-control boxed" id="${fieldName}" name="${fieldName}"></select>`);
        $list.appendTo($wrapper);

        if (field.dataSource) {
            $.getJSON(field.dataSource, data => {
                (<DropDownValues[]>data).forEach(val => {
                    $list.append(`<option value="${val.id}">${val.name}</option>`);
                });

                $list.val(field.value).change();
            });
        }

        return $list;
    }

    function generateMultiSelectInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): JQuery {
        let fieldName = generateFieldName(field, providerSection);
        
        appendLabel($wrapper, field.name, fieldName);

        let $list = $(`<select class="form-control boxed" multiple id="${fieldName}" name="${fieldName}"></select>`);
        $list.appendTo($wrapper);

        if (field.dataSource) {
            $.getJSON(field.dataSource, data => {
                (<DropDownValues[]>data).forEach(val => {
                    $list.append(`<option value="${val.id}">${val.name}</option>`);
                });

                console.log(field.value);
                $list.val(field.value).change();
            });
        }

        return $list;
    }

    function generateCronInput(field: DynamicFormField, $wrapper: JQuery, providerSection: number): JQuery {
        let fieldName = generateFieldName(field, providerSection);
        let value = field.value ? field.value : '';
        
        appendLabel($wrapper, field.name, fieldName);
        $wrapper.append(`<div class="form-control boxed"><input class="cron" type="hidden" id="${fieldName}" name="${fieldName}" value="${value}" /></div>`);

        return $wrapper.find('input');
    }

    function addValidationAttributes($field: JQuery, field: DynamicFormField): void {
        console.log($field, field);

        $field.attr('data-val', 'true');

        if (field.validation && Array.isArray(field.validation)) {
            field.validation.forEach(validation => {
                switch (validation.toLowerCase()) {
                    case 'required':
                        $field.attr('data-val-required', `The field ${field.name} is required.`);
                        break;
                }
            });
        }
    }

    function appendLabel($wrapper: JQuery, label: string, fieldName: string): void {
        $wrapper.append(`<label class="control-label" for="${fieldName}">${label}</label>`);
    }

    function generateFieldName(field: DynamicFormField, providerSection: number): string {
        if (providerSection <= 1) {
            return field.id;
        }

        return `Providers[${providerSection - 2}].${field.id}`; 
    }
}